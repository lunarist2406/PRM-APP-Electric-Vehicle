using ChargingPointService.Data;
using ChargingPointService.Models;
using ChargingPointService.Models.DTOs;
using MongoDB.Driver;
using System.Net.Http.Headers;
using System.Text.Json;

namespace ChargingPointService.Services
{
    public class ChargingPointApiService
    {
        private readonly MongoDbContext _context;
        private readonly HttpClient _httpClient;
        private readonly string _stationServiceUrl;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<ChargingPointApiService> _logger;

        public ChargingPointApiService(
            MongoDbContext context,
            IConfiguration config,
            IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor,
            ILogger<ChargingPointApiService> logger)
        {
            _context = context;
            _httpClient = httpClientFactory.CreateClient();
            _stationServiceUrl = config["STATION_API_URL"]
                ?? throw new InvalidOperationException("Missing STATION_API_URL in configuration.");

            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        private string? GetToken()
        {
            var authHeader = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();
            return (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
                ? authHeader["Bearer ".Length..]
                : null;
        }

        public async Task<List<ChargingPoint>> GetAllAsync()
        {
            _logger.LogInformation("Fetching all charging points...");

            var points = await _context.ChargingPoints.Find(_ => true).ToListAsync();

            var result = new List<ChargingPoint>();
            foreach (var point in points)
            {
                // Lấy station info từ service
                if (!string.IsNullOrEmpty(point.StationId))
                {
                    var station = await GetStationDetailsAsync(point.StationId);
                    point.StationInfo = station;
                }
                result.Add(point);
            }

            return result;
        }


        public async Task<ChargingPoint?> GetByIdAsync(string id)
        {
            _logger.LogInformation("Fetching charging point with ID: {Id}", id);

            var point = await _context.ChargingPoints.Find(x => x.Id == id).FirstOrDefaultAsync();
            if (point == null)
                return null;

            // Fill station info
            if (!string.IsNullOrEmpty(point.StationId))
            {
                var station = await GetStationDetailsAsync(point.StationId);
                point.StationInfo = station;
            }

            return point;
        }

        public async Task<ChargingPoint?> CreateAsync(ChargingPointCreateDto dto)
        {
            _logger.LogInformation("Creating new charging point for station {StationId}", dto.StationId);

            var station = await GetStationDetailsAsync(dto.StationId);
            if (station == null)
            {
                _logger.LogWarning("Station ID {StationId} not found or unreachable.", dto.StationId);
                return null;
            }

            var entity = new ChargingPoint
            {
                PointName = dto.PointName,
                StationId = dto.StationId,
                Type = dto.Type,
                Status = dto.Status,
                CreateAt = DateTime.UtcNow,
                StationInfo = station
            };

            await _context.ChargingPoints.InsertOneAsync(entity);
            _logger.LogInformation("Charging point {Id} created successfully.", entity.Id);

            return entity;
        }

        public async Task<ChargingPoint?> UpdateAsync(string id, ChargingPointUpdateDto dto)
        {
            _logger.LogInformation("🛠 Updating charging point {Id}", id);

            // 1️⃣ Kiểm tra ChargingPoint tồn tại
            var entity = await _context.ChargingPoints.Find(cp => cp.Id == id).FirstOrDefaultAsync();
            if (entity == null)
            {
                _logger.LogWarning("❌ Charging point {Id} not found for update.", id);
                return null; // ChargingPoint không tồn tại
            }

            // 2️⃣ Nếu có StationId mới, kiểm tra station tồn tại
            if (!string.IsNullOrEmpty(dto.StationId))
            {
                var station = await GetStationDetailsAsync(dto.StationId);
                if (station == null)
                {
                    _logger.LogWarning("Cannot update ChargingPoint: Station ID {StationId} not found or unreachable.", dto.StationId);
                    return null; // Station không tồn tại
                }

                entity.StationId = dto.StationId;
                entity.StationInfo = station;
            }

            // 3️⃣ Cập nhật các field còn lại
            entity.PointName = dto.PointName;
            entity.Type = dto.Type;
            entity.Status = dto.Status;
            entity.UpdateAt = DateTime.UtcNow;

            await _context.ChargingPoints.ReplaceOneAsync(cp => cp.Id == id, entity);
            _logger.LogInformation("Charging point {Id} updated successfully.", id);

            return entity;
        }


        public async Task<bool> DeleteAsync(string id)
        {
            var result = await _context.ChargingPoints.DeleteOneAsync(cp => cp.Id == id);
            var success = result.DeletedCount > 0;
            _logger.LogInformation(success
                ? "Charging point {Id} deleted successfully."
                : "Failed to delete charging point {Id} (not found).", id);

            return success;
        }

        // ✅ Refactored station verification: returns the full station info instead of just bool
        private async Task<StationResponseDto?> GetStationDetailsAsync(string stationId)
        {
            var token = GetToken();
            if (string.IsNullOrEmpty(token))
            {
                _logger.LogWarning("Missing Authorization token when verifying station {StationId}", stationId);
                return null;
            }

            try
            {
                var endpoint = $"{_stationServiceUrl}/api/Stations/{stationId}";
                var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                    return null;

                var json = await response.Content.ReadAsStringAsync();
                var station = JsonSerializer.Deserialize<StationResponseDto>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return station;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying/fetching station {StationId}", stationId);
                return null;
            }
        }
    }
}
