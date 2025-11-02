using MongoDB.Driver;
using BookingService.Models;
using Microsoft.Extensions.Configuration;

namespace BookingService.Repositories
{
    public class BookingRepository : IBookingRepository
    {
        private readonly IMongoCollection<Booking> _collection;

        public BookingRepository(IConfiguration config)
        {
            var client = new MongoClient(config["MONGO_URI"]);
            var database = client.GetDatabase(config["MONGO_DB_NAME"]);
            _collection = database.GetCollection<Booking>("bookings");
        }

        public async Task<List<Booking>> GetAllAsync(
            string? userId = null,
            string? stationId = null,
            string? vehicleId = null,
            string? chargingPointId = null,
            string? status = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            int page = 1,
            int limit = 10
        )
        {
            var filter = Builders<Booking>.Filter.Empty;

            if (!string.IsNullOrEmpty(userId))
                filter &= Builders<Booking>.Filter.Eq(x => x.UserId, userId);

            if (!string.IsNullOrEmpty(stationId))
                filter &= Builders<Booking>.Filter.Eq(x => x.StationId, stationId);

            if (!string.IsNullOrEmpty(vehicleId))
                filter &= Builders<Booking>.Filter.Eq(x => x.VehicleId, vehicleId);

            if (!string.IsNullOrEmpty(chargingPointId))
                filter &= Builders<Booking>.Filter.Eq(x => x.ChargingPointId, chargingPointId);

            if (!string.IsNullOrEmpty(status))
                filter &= Builders<Booking>.Filter.Eq(x => x.Status.ToString(), status);

            if (startDate.HasValue)
                filter &= Builders<Booking>.Filter.Gte(x => x.StartTime, startDate.Value);

            if (endDate.HasValue)
                filter &= Builders<Booking>.Filter.Lte(x => x.EndTime, endDate.Value);

            return await _collection.Find(filter)
                .Skip((page - 1) * limit)
                .Limit(limit)
                .SortByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task<Booking?> GetByIdAsync(string id)
        {
            return await _collection.Find(x => x.Id == id).FirstOrDefaultAsync();
        }

        public async Task CreateAsync(Booking booking)
        {
            await _collection.InsertOneAsync(booking);
        }

        public async Task UpdateAsync(string id, Booking booking)
        {
            await _collection.ReplaceOneAsync(x => x.Id == id, booking);
        }

        public async Task DeleteAsync(string id)
        {
            await _collection.DeleteOneAsync(x => x.Id == id);
        }
    }
}
