using MongoDB.Driver;
using UserService.Models;

namespace UserService.Data
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;

        public MongoDbContext(IConfiguration config)
        {
            var client = new MongoClient(config["MONGO_URI"]);
            _database = client.GetDatabase(config["MONGO_DB_NAME"]);
        }

        public IMongoCollection<User> Users => _database.GetCollection<User>("users");
    }
}
