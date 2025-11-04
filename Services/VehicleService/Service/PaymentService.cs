using MongoDB.Driver;
using VehicleService.Data;
using VehicleService.Models;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace VehicleService.Services
{
    public class PaymentService
    {
        private readonly MongoDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public PaymentService(MongoDbContext context, IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        // Add kWh to current payment after charging session
        // Rule: only allowed when current month's payment status is "open".
        // If status is "pending" (awaiting payment) or others, reject add.
        public async Task<bool> AddKwhToPayment(string vehicleId, string subscriptionId, decimal kwh)
        {
            // Find any payment for current month (any status)
            var now = DateTime.UtcNow;
            var currentPayment = await _context.Payments
                .Find(p => p.VehicleId == vehicleId
                    && p.SubscriptionId == subscriptionId
                    && p.BillingPeriodStart <= now
                    && p.BillingPeriodEnd >= now)
                .FirstOrDefaultAsync();

            if (currentPayment == null)
            {
                // Create new payment for current billing period (only store kWh, no calculation)
                var billingStart = GetBillingPeriodStart();
                var billingEnd = GetBillingPeriodEnd(billingStart);

                currentPayment = new Payment
                {
                    VehicleId = vehicleId,
                    SubscriptionId = subscriptionId,
                    Kwh = kwh,
                    BillingPeriodStart = billingStart,
                    BillingPeriodEnd = billingEnd,
                    Status = "open"
                };

                await _context.Payments.InsertOneAsync(currentPayment);
            }
            else
            {
                // Block adding if status is not open
                if (!string.Equals(currentPayment.Status, "open", StringComparison.OrdinalIgnoreCase))
                    return false;

                var update = Builders<Payment>.Update
                    .Inc(p => p.Kwh, kwh)
                    .Set(p => p.UpdatedAt, DateTime.UtcNow);

                await _context.Payments.UpdateOneAsync(
                    p => p.Id == currentPayment.Id,
                    update
                );
            }

            return true;
        }

        public async Task<bool> UpdatePaymentAmounts(string paymentId, decimal kwhAmount, decimal baseAmount, decimal totalAmount, decimal discountAmount = 0, decimal subtotal = 0)
        {
            var update = Builders<Payment>.Update
                .Set(p => p.KwhAmount, kwhAmount)
                .Set(p => p.BaseAmount, baseAmount)
                .Set(p => p.TotalAmount, totalAmount)
                .Set(p => p.DiscountAmount, discountAmount)
                .Set(p => p.Subtotal, subtotal > 0 ? subtotal : (kwhAmount + baseAmount))
                .Set(p => p.UpdatedAt, DateTime.UtcNow);

            var result = await _context.Payments.UpdateOneAsync(
                p => p.Id == paymentId,
                update
            );

            return result.ModifiedCount > 0;
        }

        // Get current payment (used by external services). Returns either open or pending in current month.
        public async Task<Payment?> GetCurrentPayment(string vehicleId, string subscriptionId)
        {
            var now = DateTime.UtcNow;
            return await _context.Payments
                .Find(p => p.VehicleId == vehicleId 
                    && p.SubscriptionId == subscriptionId 
                    && p.BillingPeriodStart <= now 
                    && p.BillingPeriodEnd >= now
                    && (p.Status == "open" || p.Status == "pending"))
                .FirstOrDefaultAsync();
        }

        // Get payments for monthly billing (called by SubscriptionService)
        public async Task<List<Payment>> GetPaymentsForMonthlyBilling()
        {
            var now = DateTime.UtcNow;
            var billingStart = GetBillingPeriodStart();
            
            // Find all payments that need to be finalized for the previous month
            var previousMonthStart = billingStart.AddMonths(-1);
            var previousMonthEnd = billingStart.AddMilliseconds(-1);

            return await _context.Payments
                .Find(p => p.BillingPeriodStart >= previousMonthStart 
                    && p.BillingPeriodEnd <= previousMonthEnd
                    && p.Status == "open")
                .ToListAsync();
        }

        private DateTime GetBillingPeriodStart()
        {
            var now = DateTime.UtcNow;
            return new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        }

        private DateTime GetBillingPeriodEnd(DateTime start)
        {
            return start.AddMonths(1).AddMilliseconds(-1);
        }

        // Get all payments for a vehicle
        public async Task<List<Payment>> GetPaymentsByVehicleId(string vehicleId)
        {
            return await _context.Payments
                .Find(p => p.VehicleId == vehicleId)
                .SortByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        // Get payment by ID
        public async Task<Payment?> GetPaymentById(string id)
        {
            return await _context.Payments.Find(p => p.Id == id).FirstOrDefaultAsync();
        }

        // Update payment status (e.g., mark as paid)
        public async Task<bool> UpdatePaymentStatus(string paymentId, string status)
        {
            var update = Builders<Payment>.Update
                .Set(p => p.Status, status)
                .Set(p => p.UpdatedAt, DateTime.UtcNow);

            var result = await _context.Payments.UpdateOneAsync(
                p => p.Id == paymentId,
                update
            );

            return result.ModifiedCount > 0;
        }
    }
}

