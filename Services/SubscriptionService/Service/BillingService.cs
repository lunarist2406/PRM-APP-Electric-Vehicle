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

        public async Task<ChargingSession?> EndSession(string sessionId, decimal batteryNeededKwh, string stationId, decimal stationKwh)
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

            // Calculate cost based on business logic
            var cost = CalculateSessionCost(kwhUsed, batteryNeededKwh, subscriptionPlan.KwhPrice);

            // Update session
            session.EndTime = endTime;
            session.DurationMinutes = duration;
            session.KwhUsed = kwhUsed;
            session.BatteryNeededKwh = batteryNeededKwh;
            session.Cost = cost;
            session.Status = "completed";

            await _subscriptionService.UpdateSession(sessionId, session);

            // kWh will be saved to Payment model in VehicleService by FE
            // No usage tracking here anymore

            return session;
        }

        private decimal CalculateSessionCost(decimal kwhUsed, decimal batteryNeededKwh, decimal kwhPrice)
        {
            var energyToCharge = kwhUsed <= batteryNeededKwh ? kwhUsed : batteryNeededKwh;
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
                            var price = station.TryGetProperty("pricePerKwh", out var p) ? p.GetDecimal() : 0m;
                            var sessionKwh = session.KwhUsed;
                            kwhAmount += sessionKwh * price;
                            summedKwh += sessionKwh;
                        }
                    }
                    catch { }
                }
            }

            // Fallback to plan price if no session data fetched
            if (kwhAmount == 0m && totalKwhForAllVehicles > 0)
            {
                kwhAmount = totalKwhForAllVehicles * subscriptionPlan.KwhPrice;
                summedKwh = totalKwhForAllVehicles;
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
                ?? "http://localhost:5004";
            
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
    }
}

