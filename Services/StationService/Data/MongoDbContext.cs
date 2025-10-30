using MongoDB.Driver;
using StationService.Models;

namespace StationService.Data
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;

        public MongoDbContext()
        {
            var mongoUri = Environment.GetEnvironmentVariable("MONGO_URI");
            if (string.IsNullOrEmpty(mongoUri))
                throw new Exception("Missing MONGO_URI environment variable!");
            var dbName = Environment.GetEnvironmentVariable("MONGO_DB_NAME") ?? "ev_station";

            var client = new MongoClient(mongoUri);
            _database = client.GetDatabase(dbName);
        }
        public IMongoCollection<Station> Stations => _database.GetCollection<Station>("stations");
    }
}
