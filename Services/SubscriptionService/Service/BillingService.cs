using SubscriptionService.Data;
using SubscriptionService.Model;
using SubscriptionService.Model.DTOs;
using System.Text.Json;
using System.Text;

namespace SubscriptionService.Service
{
    public class BillingService
    {
        private readonly MongoDbContext _context;
        private readonly SubscriptionDataService _subscriptionService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public BillingService(MongoDbContext context, SubscriptionDataService subscriptionService, IHttpClientFactory httpClientFactory, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _subscriptionService = subscriptionService;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ChargingSession?> EndSession(string sessionId, decimal batteryNeededKwh, decimal? actualKwh, string stationId, decimal stationKwh)
        {
            var session = await _subscriptionService.GetSessionById(sessionId);
            if (session == null || session.Status != "ongoing") return null;

            var vehicleSubscription = await _subscriptionService.GetVehicleSubscriptionById(session.VehicleSubscriptionId);
            if (vehicleSubscription == null) return null;

            var subscriptionPlan = await _subscriptionService.GetPlanById(vehicleSubscription.SubscriptionId);
            if (subscriptionPlan == null) return null;

            // Calculate duration
            var endTime = DateTime.UtcNow;
            var duration = (int)(endTime - session.StartTime).TotalMinutes;

            // Calculate kWh used: duration in hours * station kwh capacity
            var kwhUsed = (duration / 60.0m) * stationKwh;

            // Validate actualKwh before calculating cost
            decimal? validatedActualKwh = actualKwh;
            if (actualKwh.HasValue && batteryNeededKwh > 0 && actualKwh.Value > batteryNeededKwh)
            {
                Console.WriteLine($"⚠️ Warning: actualKwh ({actualKwh.Value}) > batteryNeededKwh ({batteryNeededKwh}). Using batteryNeededKwh instead.");
                validatedActualKwh = batteryNeededKwh; // Cap at batteryNeededKwh
            }

            // Calculate cost based on business logic
            // Priority: actualKwh (thực tế) > batteryNeededKwh (dự định) > calculated kwhUsed (từ duration)
            var cost = CalculateSessionCost(validatedActualKwh, batteryNeededKwh, kwhUsed, subscriptionPlan.KwhPrice);

            // Update session
            session.EndTime = endTime;
            session.DurationMinutes = duration;
            session.KwhUsed = kwhUsed; // Keep calculated value for reference
            session.ActualKwh = validatedActualKwh; // Store actual kWh from device (source of truth)
            session.BatteryNeededKwh = batteryNeededKwh;
            session.Cost = cost;
            session.Status = "completed";

            await _subscriptionService.UpdateSession(sessionId, session);

            // Automatically add kWh to Payment in VehicleService
            // actualKwh: kWh thực tế đã sạc (từ thiết bị, có thể < batteryNeededKwh nếu rút sớm)
            // batteryNeededKwh: kWh dự định sạc để đầy pin (100% - % hiện tại)
            // Priority: actualKwh (thực tế) > batteryNeededKwh (dự định) > calculated kwhUsed (từ duration)
            // validatedActualKwh đã được validate ở trên (capped at batteryNeededKwh if needed)
            
            var kwhToAdd = validatedActualKwh ?? (batteryNeededKwh > 0 ? batteryNeededKwh : kwhUsed);
            
            Console.WriteLine($"🔋 End session kWh: actualKwh={actualKwh}, batteryNeededKwh={batteryNeededKwh}, calculatedKwhUsed={kwhUsed}, using={kwhToAdd}");
            
            if (kwhToAdd > 0)
            {
                try
                {
                    var vehicleServiceUrl = Environment.GetEnvironmentVariable("VEHICLE_SERVICE_URL") 
                        ?? _configuration["VehicleService:BaseUrl"] 
                        ?? "http://localhost:5003";

                    var httpClient = _httpClientFactory.CreateClient();
                    httpClient.Timeout = TimeSpan.FromSeconds(10);

                    // Forward Authorization header (Bearer token) to VehicleService
                    var authHeader = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();
                    if (!string.IsNullOrEmpty(authHeader))
                    {
                        var token = authHeader.StartsWith("Bearer ") ? authHeader.Substring("Bearer ".Length) : authHeader;
                        httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                    }

                    var addKwhResponse = await httpClient.PostAsync(
                        $"{vehicleServiceUrl}/api/payment/add-kwh",
                        new StringContent(JsonSerializer.Serialize(new
                        {
                            vehicleId = vehicleSubscription.VehicleId,
                            subscriptionId = vehicleSubscription.SubscriptionId,
                            kwh = kwhToAdd
                        }), System.Text.Encoding.UTF8, "application/json")
                    );

                    if (addKwhResponse.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"✅ Automatically added {kwhToAdd} kWh to payment for vehicle {vehicleSubscription.VehicleId}");
                    }
                    else
                    {
                        var errorContent = await addKwhResponse.Content.ReadAsStringAsync();
                        Console.WriteLine($"⚠️ Failed to auto-add kWh to payment: {addKwhResponse.StatusCode} - {errorContent}");
                        // Session is already updated, but payment add failed
                        // FE can manually call add-kwh endpoint if needed
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ Error auto-adding kWh to payment: {ex.Message}");
                    Console.WriteLine($"   StackTrace: {ex.StackTrace}");
                    // Don't fail the session end if payment add fails - session is already saved
                    // FE can manually call add-kwh endpoint if needed
                }
            }

            return session;
        }

        private decimal CalculateSessionCost(decimal? actualKwh, decimal batteryNeededKwh, decimal calculatedKwhUsed, decimal kwhPrice)
        {
            // Priority: actualKwh (thực tế từ thiết bị) > batteryNeededKwh (dự định) > calculatedKwhUsed (từ duration)
            decimal energyToCharge;
            
            if (actualKwh.HasValue && actualKwh.Value > 0)
            {
                // Use actual kWh charged (may be less than batteryNeededKwh if user unplugged early)
                energyToCharge = actualKwh.Value;
            }
            else if (batteryNeededKwh > 0)
            {
                // Use planned kWh (but cap at calculated if calculated is less)
                energyToCharge = calculatedKwhUsed <= batteryNeededKwh ? calculatedKwhUsed : batteryNeededKwh;
            }
            else
            {
                // Fallback to calculated kWh
                energyToCharge = calculatedKwhUsed;
            }
            
            return energyToCharge * kwhPrice;
        }

        // Parse discount from string (e.g., "15%" -> 0.15)
        private decimal ParseDiscount(string? discountStr)
        {
            if (string.IsNullOrEmpty(discountStr))
                return 0;

            // Remove % and parse
            discountStr = discountStr.Trim().Replace("%", "").Trim();
            if (decimal.TryParse(discountStr, out var discountPercent))
            {
                return discountPercent / 100; // Convert 15 -> 0.15
            }

            return 0;
        }

        // Calculate monthly billing using StationService pricePerKwh per session
        // vehicleSubscriptionId is required to fetch sessions; planId is resolved from it
        public async Task<MonthlyBillingResult> CalculateMonthlyBillingForSubscription(string vehicleSubscriptionId, decimal totalKwhForAllVehicles)
        {
            // Resolve plan from vehicleSubscriptionId
            var vsub = await _subscriptionService.GetVehicleSubscriptionById(vehicleSubscriptionId);
            if (vsub == null) throw new Exception("Vehicle subscription not found");
            var subscriptionPlan = await _subscriptionService.GetPlanById(vsub.SubscriptionId);
            if (subscriptionPlan == null)
                throw new Exception("Subscription plan not found");

            // Billing period = current month
            var periodStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var periodEnd = periodStart.AddMonths(1).AddMilliseconds(-1);

            var sessions = await _subscriptionService.GetSessionsBySubscriptionId(vehicleSubscriptionId);
            var periodSessions = sessions.Where(s => s.StartTime >= periodStart && s.StartTime <= periodEnd).ToList();

            decimal kwhAmount = 0m;
            decimal summedKwh = 0m;

            if (periodSessions.Any())
            {
                var httpClient = _httpClientFactory.CreateClient();
                httpClient.Timeout = TimeSpan.FromSeconds(10);

                var authHeader = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();
                if (!string.IsNullOrEmpty(authHeader))
                {
                    var token = authHeader.StartsWith("Bearer ") ? authHeader.Substring("Bearer ".Length) : authHeader;
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }

                var stationServiceUrl = Environment.GetEnvironmentVariable("STATION_SERVICE_URL")
                    ?? _configuration["StationService:BaseUrl"]
                    ?? "http://localhost:5002";

                foreach (var session in periodSessions)
                {
                    try
                    {
                        var resp = await httpClient.GetAsync($"{stationServiceUrl}/api/Stations/{session.StationId}");
                        if (resp.IsSuccessStatusCode)
                        {
                            var json = await resp.Content.ReadAsStringAsync();
                            var station = JsonSerializer.Deserialize<JsonElement>(json);
                            var price = ExtractStationPrice(station);
                            
                            // Use actualKwh if available (from device), otherwise fallback to KwhUsed (calculated)
                            var sessionKwh = session.ActualKwh ?? session.KwhUsed;
                            
                            if (price > 0 && sessionKwh > 0)
                            {
                                kwhAmount += sessionKwh * price;
                                summedKwh += sessionKwh;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"⚠️ Error fetching station {session.StationId}: {ex.Message}");
                    }
                }
            }

            // If we calculated from sessions, use average price and apply to totalKwhFromPayment (source of truth)
            // Payment kWh is the actual kWh from FE (add-kwh), sessions are just for price reference
            if (kwhAmount > 0m && summedKwh > 0m && totalKwhForAllVehicles > 0m)
            {
                // Calculate average price from sessions
                var avgPrice = kwhAmount / summedKwh;
                // Recalculate using totalKwhFromPayment (actual kWh from FE)
                kwhAmount = totalKwhForAllVehicles * avgPrice;
                summedKwh = totalKwhForAllVehicles;
            }

            // Fallback: if no sessions found or no price from stations, use totalKwh from Payment
            // Priority: 1) Plan kwh_price, 2) Latest station price, 3) Error
            if (kwhAmount == 0m && totalKwhForAllVehicles > 0)
            {
                // Priority 1: Use plan's kwh_price if available
                if (subscriptionPlan.KwhPrice > 0)
                {
                    kwhAmount = totalKwhForAllVehicles * subscriptionPlan.KwhPrice;
                    summedKwh = totalKwhForAllVehicles;
                }
                else
                {
                    // Try to get price from any recent session's station
                    var allSessions = await _subscriptionService.GetSessionsBySubscriptionId(vehicleSubscriptionId);
                    if (allSessions.Any())
                    {
                        var latestSession = allSessions.OrderByDescending(s => s.StartTime).First();
                        try
                        {
                            var httpClient = _httpClientFactory.CreateClient();
                            httpClient.Timeout = TimeSpan.FromSeconds(5);
                            
                            var authHeader = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();
                            if (!string.IsNullOrEmpty(authHeader))
                            {
                                var token = authHeader.StartsWith("Bearer ") ? authHeader.Substring("Bearer ".Length) : authHeader;
                                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                            }

                            var stationServiceUrl = Environment.GetEnvironmentVariable("STATION_SERVICE_URL")
                                ?? _configuration["StationService:BaseUrl"]
                                ?? "http://localhost:5002";

                            var resp = await httpClient.GetAsync($"{stationServiceUrl}/api/Stations/{latestSession.StationId}");
                            
                            if (resp.IsSuccessStatusCode)
                            {
                                var json = await resp.Content.ReadAsStringAsync();
                                var station = JsonSerializer.Deserialize<JsonElement>(json);
                                var price = ExtractStationPrice(station);
                                
                                if (price > 0)
                                {
                                    kwhAmount = totalKwhForAllVehicles * price;
                                    summedKwh = totalKwhForAllVehicles;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"⚠️ Error fetching latest station for fallback: {ex.Message}");
                        }
                    }
                }
            }

            // Ensure TotalKwh is always from payment (source of truth), even if price calculation failed
            if (summedKwh == 0m && totalKwhForAllVehicles > 0m)
            {
                summedKwh = totalKwhForAllVehicles;
                // kwhAmount remains 0 if no price found
            }

            var baseAmount = subscriptionPlan.Price;
            var subtotal = kwhAmount + baseAmount;

            var discountPercent = ParseDiscount(subscriptionPlan.Discount);
            var discountAmount = subtotal * discountPercent;
            var totalAmount = subtotal - discountAmount;

            // Rounding rules: prices 4 d.p., money 2 d.p. (AwayFromZero)
            var roundedKwhAmount = Math.Round(kwhAmount, 2, MidpointRounding.AwayFromZero);
            var roundedBaseAmount = Math.Round(baseAmount, 2, MidpointRounding.AwayFromZero);
            var roundedSubtotal = Math.Round(subtotal, 2, MidpointRounding.AwayFromZero);
            var roundedDiscount = Math.Round(discountAmount, 2, MidpointRounding.AwayFromZero);
            var roundedTotal = Math.Round(totalAmount, 2, MidpointRounding.AwayFromZero);
            var roundedAvgPrice = summedKwh > 0 ? Math.Round(roundedKwhAmount / summedKwh, 4, MidpointRounding.AwayFromZero) : 0;

            return new MonthlyBillingResult
            {
                SubscriptionId = vsub.SubscriptionId,
                TotalKwh = summedKwh,
                KwhPrice = roundedAvgPrice,
                KwhAmount = roundedKwhAmount,
                BaseAmount = roundedBaseAmount,
                DiscountPercent = discountPercent,
                DiscountAmount = roundedDiscount,
                Subtotal = roundedSubtotal,
                TotalAmount = roundedTotal
            };
        }

        // Calculate monthly billing for single vehicle (legacy, for backward compatibility)
        public async Task<MonthlyBillingResult> CalculateMonthlyBilling(string vehicleId, string subscriptionId, decimal totalKwh)
        {
            var vehicleSubscription = await _subscriptionService.GetActiveSubscriptionByVehicleId(vehicleId);
            if (vehicleSubscription == null || vehicleSubscription.SubscriptionId != subscriptionId)
                throw new Exception("Vehicle subscription not found");

            // Here subscriptionId is plan id; use vehicleSubscription.Id instead for session lookup
            return await CalculateMonthlyBillingForSubscription(vehicleSubscription.Id!, totalKwh);
        }

        // Generate monthly bills for all active subscriptions
        // Mỗi subscription = 1 xe, tính riêng biệt
        public async Task<List<MonthlyBillingResult>> GenerateMonthlyBillsForAllSubscriptions()
        {
            var results = new List<MonthlyBillingResult>();

            // Get all active subscriptions (mỗi subscription = 1 xe)
            var allSubscriptions = await _subscriptionService.GetAllVehicleSubscriptions();
            var activeSubscriptions = allSubscriptions
                .Where(s => s.PaymentStatus != "cancelled")
                .ToList();

            var vehicleServiceUrl = Environment.GetEnvironmentVariable("VEHICLE_SERVICE_URL") 
                ?? _configuration["VehicleService:BaseUrl"] 
                ?? "http://localhost:5002";

            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                httpClient.Timeout = TimeSpan.FromSeconds(10);

                // Forward Authorization header (Bearer token) to VehicleService
                var authHeader = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();
                if (!string.IsNullOrEmpty(authHeader))
                {
                    var token = authHeader.StartsWith("Bearer ") ? authHeader.Substring("Bearer ".Length) : authHeader;
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }

                foreach (var vehicleSubscription in activeSubscriptions)
                {
                    try
                    {
                        var vehicleId = vehicleSubscription.VehicleId;
                        var planId = vehicleSubscription.SubscriptionId;

                        // Get current payment for this vehicle
                        var paymentResponse = await httpClient.GetAsync(
                            $"{vehicleServiceUrl}/api/payment/current/{vehicleId}/{planId}");

                        if (paymentResponse.IsSuccessStatusCode)
                        {
                            var paymentJson = await paymentResponse.Content.ReadAsStringAsync();
                            var payment = JsonSerializer.Deserialize<JsonElement>(paymentJson);

                            if (payment.TryGetProperty("kwh", out var kwhElement) && 
                                payment.TryGetProperty("id", out var idElement))
                            {
                                var totalKwh = kwhElement.GetDecimal();
                                var paymentId = idElement.GetString() ?? "";

                                if (totalKwh > 0 && !string.IsNullOrEmpty(paymentId))
                                {
                                    // Calculate billing for this single vehicle subscription
                                    // Formula: (kWh * kwhPrice + baseAmount) - discount
                                    var billingResult = await CalculateMonthlyBillingForSubscription(
                                        vehicleSubscription.Id!, 
                                        totalKwh
                                    );

                                    // Update payment amounts
                                    var updateResponse = await httpClient.PatchAsync(
                                        $"{vehicleServiceUrl}/api/payment/{paymentId}/amounts",
                                        new StringContent(JsonSerializer.Serialize(new
                                        {
                                            kwhAmount = billingResult.KwhAmount,
                                            baseAmount = billingResult.BaseAmount,
                                            totalAmount = billingResult.TotalAmount,
                                            discountAmount = billingResult.DiscountAmount,
                                            subtotal = billingResult.Subtotal
                                        }), System.Text.Encoding.UTF8, "application/json")
                                    );

                                    if (updateResponse.IsSuccessStatusCode)
                                    {
                                        billingResult.VehicleId = vehicleId;
                                        results.Add(billingResult);

                                        // Lock payment: set status to pending (no more add-kWh)
                                        await httpClient.PatchAsync(
                                            $"{vehicleServiceUrl}/api/payment/{paymentId}/status",
                                            new StringContent(JsonSerializer.Serialize(new { status = "pending" }), System.Text.Encoding.UTF8, "application/json")
                                        );
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"⚠️ Error processing vehicle subscription {vehicleSubscription.Id}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error generating monthly bills: {ex.Message}");
            }

            return results;
        }

        public class MonthlyBillingResult
        {
            public string? VehicleId { get; set; } // Null if calculated for entire subscription
            public string SubscriptionId { get; set; } = string.Empty;
            public decimal TotalKwh { get; set; }
            public decimal KwhPrice { get; set; }
            public decimal KwhAmount { get; set; }
            public decimal BaseAmount { get; set; }
            public decimal DiscountPercent { get; set; }
            public decimal DiscountAmount { get; set; }
            public decimal Subtotal { get; set; } // Before discount: kWhAmount + BaseAmount
            public decimal TotalAmount { get; set; } // After discount: Subtotal - DiscountAmount
        }

        public async Task<decimal> GetStationKwh(string stationId)
        {
            const decimal DEFAULT_STATION_KWH = 65;
            
            var stationServiceUrl = Environment.GetEnvironmentVariable("STATION_SERVICE_URL") 
                ?? _configuration["StationService:BaseUrl"] 
                ?? "http://localhost:5002";
            
            try
            {
                var httpClient = _httpClientFactory.CreateClient();
                httpClient.Timeout = TimeSpan.FromSeconds(5); // 5 second timeout
                
                var response = await httpClient.GetAsync($"{stationServiceUrl}/api/stations/{stationId}");
                
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var station = JsonSerializer.Deserialize<JsonElement>(json);
                    
                    if (station.TryGetProperty("kwh", out var kwhElement))
                        return kwhElement.GetDecimal();
                    
                    if (station.TryGetProperty("power", out var powerElement))
                        return powerElement.GetDecimal();
                        
                    if (station.TryGetProperty("capacity", out var capacityElement))
                        return capacityElement.GetDecimal();
                }
                
                return DEFAULT_STATION_KWH;
            }
            catch (HttpRequestException ex)
            {
                // Station service not available or network error
                Console.WriteLine($"⚠️ Cannot reach StationService: {ex.Message}");
                return DEFAULT_STATION_KWH;
            }
            catch (TaskCanceledException)
            {
                // Timeout
                Console.WriteLine($"⚠️ StationService request timeout");
                return DEFAULT_STATION_KWH;
            }
            catch (Exception ex)
            {
                // Any other error
                Console.WriteLine($"⚠️ Error getting station info: {ex.Message}");
                return DEFAULT_STATION_KWH;
            }
        }

        private decimal ExtractStationPrice(JsonElement station)
        {
            try
            {
                string[] candidateKeys = new[]
                {
                    "pricePerKwh",
                    "price_per_kwh",
                    "pricePerKWh",
                    "price_per_KWh",
                    "price"
                };

                foreach (var key in candidateKeys)
                {
                    if (station.TryGetProperty(key, out var value) && value.ValueKind == JsonValueKind.Number)
                    {
                        return value.GetDecimal();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Failed to parse station price: {ex.Message}");
            }

            return 0m;
        }
    }
}

