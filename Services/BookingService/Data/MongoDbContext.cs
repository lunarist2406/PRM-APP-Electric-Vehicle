using MongoDB.Driver;
using BookingService.Models; 

namespace BookingService.Data
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
            var dbName = Environment.GetEnvironmentVariable("MONGO_DB_NAME") ?? "ev_booking";

            // Tạo Mongo client
            var client = new MongoClient(mongoUri);
            _database = client.GetDatabase(dbName);
        }

        // 👉 Collection chính cho Booking
        public IMongoCollection<Booking> Bookings =>
            _database.GetCollection<Booking>("bookings");

        // (optional) nếu sau này muốn gọi sang entity khác (ex: user hoặc vehicle)
        // public IMongoCollection<User> Users => _database.GetCollection<User>("users");
        // public IMongoCollection<Station> Stations => _database.GetCollection<Station>("stations");
    }
}
