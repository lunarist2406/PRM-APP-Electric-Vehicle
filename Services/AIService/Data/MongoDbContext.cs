using MongoDB.Driver;
using AIService.Models;

namespace AIService.Data
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;

        public MongoDbContext()
        {
            var mongoUri = Environment.GetEnvironmentVariable("MONGO_URI");
            if (string.IsNullOrEmpty(mongoUri))
                throw new Exception("Missing MONGO_URI environment variable!");

            var dbName = Environment.GetEnvironmentVariable("MONGO_DB_NAME") ?? "ev_ai";

            var client = new MongoClient(mongoUri);
            _database = client.GetDatabase(dbName);
        }

        // 🧠 Lưu lịch sử hội thoại AI
        public IMongoCollection<ChatHistory> ChatHistories =>
            _database.GetCollection<ChatHistory>("chat_history");

        // 🔌 Nếu AI cần tham chiếu dữ liệu trạm
        // public IMongoCollection<Station> Stations =>
        //     _database.GetCollection<Station>("stations");
    }
}
