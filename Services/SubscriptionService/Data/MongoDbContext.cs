using MongoDB.Driver;
using SubscriptionService.Model;
using Microsoft.Extensions.Configuration;

namespace SubscriptionService.Data
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;

        public MongoDbContext(IConfiguration configuration)
        {
            var mongoUri = Environment.GetEnvironmentVariable("MONGO_URI") 
                ?? configuration["MONGO_URI"];
            
            if (string.IsNullOrEmpty(mongoUri))
                throw new Exception("❌ Missing MONGO_URI! Set in environment variable or appsettings.json");

            var dbName = Environment.GetEnvironmentVariable("MONGO_DB_NAME") 
                ?? configuration["MONGO_DB_NAME"] 
                ?? "ev_subscription";
            
            var client = new MongoClient(mongoUri);
            _database = client.GetDatabase(dbName);
        }

        public IMongoCollection<SubscriptionPlan> SubscriptionPlans => 
            _database.GetCollection<SubscriptionPlan>("subscription_plans");

        public IMongoCollection<VehicleSubscription> VehicleSubscriptions => 
            _database.GetCollection<VehicleSubscription>("vehicle_subscriptions");

        // VehicleSubscriptionUsage no longer needed - payment tracking moved to VehicleService
        // public IMongoCollection<VehicleSubscriptionUsage> VehicleSubscriptionUsages => 
        //     _database.GetCollection<VehicleSubscriptionUsage>("vehicle_subscription_usages");

        public IMongoCollection<ChargingSession> ChargingSessions => 
            _database.GetCollection<ChargingSession>("charging_sessions");
    }
}

