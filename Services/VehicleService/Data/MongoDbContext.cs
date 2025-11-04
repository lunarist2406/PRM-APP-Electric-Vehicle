using MongoDB.Driver;
using VehicleService.Models;

namespace VehicleService.Data
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;

        public MongoDbContext()
        {
            var mongoUri = Environment.GetEnvironmentVariable("MONGO_URI");
            if (string.IsNullOrEmpty(mongoUri))
                throw new Exception("❌ Missing MONGO_URI environment variable!");

            var dbName = Environment.GetEnvironmentVariable("MONGO_DB_NAME") ?? "ev_vehicle";
            var client = new MongoClient(mongoUri);
            _database = client.GetDatabase(dbName);
        }

        public IMongoCollection<Vehicle> Vehicles => _database.GetCollection<Vehicle>("vehicles");
        public IMongoCollection<Payment> Payments => _database.GetCollection<Payment>("payments");
    }
}
