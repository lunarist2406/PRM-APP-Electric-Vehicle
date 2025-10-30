using StationService.Data;
using StationService.Models;
using StationService.DTOs;
using MongoDB.Driver;

namespace StationService.Services
{
    public class StationService
    {
        private readonly IMongoCollection<Station> _stations;

        public StationService(MongoDbContext context)
        {
            _stations = context.Stations;
        }

        public async Task<Station> CreateAsync(Station station)
        {
            await _stations.InsertOneAsync(station);
            return station;
        }

        public async Task<List<Station>> GetAllAsync() =>
            await _stations.Find(_ => true).ToListAsync();

        public async Task<Station?> GetByIdAsync(string id) =>
            await _stations.Find(s => s.Id == id).FirstOrDefaultAsync();

        public async Task<Station?> UpdateAsync(string id, StationUpdateDto dto)
        {
            var update = Builders<Station>.Update
                .Set(s => s.Name, dto.Name)
                .Set(s => s.Address, dto.Address)
                .Set(s => s.Latitude, dto.Latitude)
                .Set(s => s.Longitude, dto.Longitude)
                .Set(s => s.PowerCapacity, dto.PowerCapacity)
                .Set(s => s.PricePerKwh, dto.PricePerKwh)
                .Set(s => s.Status, dto.Status);

            var options = new FindOneAndUpdateOptions<Station>
            {
                ReturnDocument = ReturnDocument.After
            };

            return await _stations.FindOneAndUpdateAsync(s => s.Id == id, update, options);
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var result = await _stations.DeleteOneAsync(s => s.Id == id);
            return result.DeletedCount > 0;
        }
    }
}
