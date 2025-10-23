using MongoDB.Driver;
using CompanyService.Models;

namespace CompanyService.Data
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;
        public MongoDbContext()
        {
            var mongoUri = Environment.GetEnvironmentVariable("MONGO_URI");
            if (string.IsNullOrEmpty(mongoUri))
                throw new Exception("❌ Missing MONGO_URI in environment variables.");

            var dbName = Environment.GetEnvironmentVariable("MONGO_DB_NAME") ?? "ev_company";
            var client = new MongoClient(mongoUri);
            _database = client.GetDatabase(dbName);
        }

        public IMongoCollection<Company> Companies => _database.GetCollection<Company>("companies");
    }
}
