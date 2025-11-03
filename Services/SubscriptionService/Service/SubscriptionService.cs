using MongoDB.Driver;
using SubscriptionService.Data;
using SubscriptionService.Model;
using SubscriptionService.Model.DTOs;

namespace SubscriptionService.Service
{
    public class SubscriptionDataService
    {
        private readonly MongoDbContext _context;

        public SubscriptionDataService(MongoDbContext context)
        {
            _context = context;
        }

        // Subscription Plans
        public async Task<List<SubscriptionPlan>> GetAllPlans()
            => await _context.SubscriptionPlans.Find(_ => true).ToListAsync();

        public async Task<SubscriptionPlan?> GetPlanById(string id)
            => await _context.SubscriptionPlans.Find(p => p.Id == id).FirstOrDefaultAsync();

        public async Task CreatePlan(SubscriptionPlan plan)
            => await _context.SubscriptionPlans.InsertOneAsync(plan);

        public async Task<bool> UpdatePlan(string id, SubscriptionPlan plan)
        {
            var result = await _context.SubscriptionPlans.ReplaceOneAsync(p => p.Id == id, plan);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> DeletePlan(string id)
        {
            var result = await _context.SubscriptionPlans.DeleteOneAsync(p => p.Id == id);
            return result.DeletedCount > 0;
        }

        // Vehicle Subscriptions
        // Mỗi subscription chỉ gắn với 1 xe
        public async Task<VehicleSubscription?> CreateVehicleSubscription(RegisterSubscriptionDto dto)
        {
            var plan = await GetPlanById(dto.SubscriptionId);
            if (plan == null) return null;

            // Check if vehicle already has active subscription
            var existingVehicleSubscription = await GetActiveSubscriptionByVehicleId(dto.VehicleId);
            if (existingVehicleSubscription != null)
            {
                throw new Exception("Vehicle already has an active subscription");
            }

            var subscription = new VehicleSubscription
            {
                VehicleId = dto.VehicleId,
                SubscriptionId = dto.SubscriptionId,
                AutoRenew = dto.AutoRenew,
                PaymentStatus = "pending"
            };

            await _context.VehicleSubscriptions.InsertOneAsync(subscription);
            return subscription;
        }

        public async Task<VehicleSubscription?> GetVehicleSubscriptionById(string id)
            => await _context.VehicleSubscriptions.Find(s => s.Id == id).FirstOrDefaultAsync();

        public async Task<VehicleSubscription?> GetActiveSubscriptionByVehicleId(string vehicleId)
        {
            return await _context.VehicleSubscriptions
                .Find(s => s.VehicleId == vehicleId && s.PaymentStatus != "cancelled")
                .FirstOrDefaultAsync();
        }

        public async Task<List<VehicleSubscription>> GetVehicleSubscriptionsByVehicleId(string vehicleId)
            => await _context.VehicleSubscriptions.Find(s => s.VehicleId == vehicleId).ToListAsync();

        public async Task<List<VehicleSubscription>> GetAllVehicleSubscriptions()
            => await _context.VehicleSubscriptions.Find(_ => true).ToListAsync();

        public async Task<bool> UpdateVehicleSubscription(string id, VehicleSubscription subscription)
        {
            var result = await _context.VehicleSubscriptions.ReplaceOneAsync(s => s.Id == id, subscription);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> DeleteVehicleSubscription(string id)
        {
            var result = await _context.VehicleSubscriptions.DeleteOneAsync(s => s.Id == id);
            return result.DeletedCount > 0;
        }

        // Charging Sessions
        public async Task<ChargingSession?> CreateSession(string vehicleSubscriptionId, string stationId, int spotId, int? bookingId = null)
        {
            var session = new ChargingSession
            {
                BookingId = bookingId,
                SpotId = spotId,
                VehicleSubscriptionId = vehicleSubscriptionId,
                StationId = stationId,
                StartTime = DateTime.UtcNow,
                Status = "ongoing"
            };

            await _context.ChargingSessions.InsertOneAsync(session);
            return session;
        }

        public async Task<ChargingSession?> GetSessionById(string id)
            => await _context.ChargingSessions.Find(s => s.Id == id).FirstOrDefaultAsync();

        public async Task<bool> UpdateSession(string id, ChargingSession session)
        {
            var result = await _context.ChargingSessions.ReplaceOneAsync(s => s.Id == id, session);
            return result.ModifiedCount > 0;
        }

        public async Task<List<ChargingSession>> GetSessionsBySubscriptionId(string vehicleSubscriptionId)
            => await _context.ChargingSessions.Find(s => s.VehicleSubscriptionId == vehicleSubscriptionId).ToListAsync();

        // Payment tracking is now handled in VehicleService
    }
}

