using MongoDB.Driver;
using PaymentService.Models;

namespace PaymentService.Data
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;

        public MongoDbContext()
        {
            // Lấy URI từ biến môi trường
            var mongoUri = Environment.GetEnvironmentVariable("MONGO_URI");
            if (string.IsNullOrEmpty(mongoUri))
                throw new Exception("❌ Missing MONGO_URI environment variable!");

            // Tên database (nếu không có ENV thì fallback mặc định)
            var dbName = Environment.GetEnvironmentVariable("MONGO_DB_NAME") ?? "ev_payment";

            // Tạo Mongo client
            var client = new MongoClient(mongoUri);
            _database = client.GetDatabase(dbName);
        }

        // 👉 Collection chính cho Payment
        public IMongoCollection<Payment> Payments =>
            _database.GetCollection<Payment>("payments");

        // (Optional) nếu sau này muốn lưu thêm entity khác
        // public IMongoCollection<User> Users => _database.GetCollection<User>("users");
        // public IMongoCollection<Vehicle> Vehicles => _database.GetCollection<Vehicle>("vehicles");
    }
}
