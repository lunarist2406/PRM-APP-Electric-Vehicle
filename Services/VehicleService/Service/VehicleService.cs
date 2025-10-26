using MongoDB.Driver;
using VehicleService.Models;
using VehicleService.Data;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http;
using MongoDB.Bson;

namespace VehicleService.Services
{
    public class VehicleDataService
    {
        private readonly IMongoCollection<Vehicle> _vehicles;
        private readonly HttpClient _httpClient;
        private readonly string _userServiceUrl;
        private readonly string _companyServiceUrl;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public VehicleDataService(MongoDbContext context, IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
        {
            _vehicles = context.Vehicles;
            _httpClient = httpClientFactory.CreateClient();
            _httpContextAccessor = httpContextAccessor;

            _userServiceUrl = Environment.GetEnvironmentVariable("USER_SERVICE_URL")
                ?? throw new Exception("USER_SERVICE_URL not configured");
            _companyServiceUrl = Environment.GetEnvironmentVariable("COMPANY_SERVICE_URL")
                ?? throw new Exception("COMPANY_SERVICE_URL not configured");
        }

        // Lấy token từ header Authorization
        private string? GetToken()
        {
            var authHeader = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();
            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
            {
                return authHeader["Bearer ".Length..];
            }
            return null;
        }

        // ================= CRUD =================
        public async Task<List<Vehicle>> GetAllAsync(int page = 1, int limit = 10) =>
            await _vehicles.Find(_ => true)
                           .Skip((page - 1) * limit)
                           .Limit(limit)
                           .ToListAsync();

        public async Task<Vehicle?> GetByIdAsync(string id) =>
            await _vehicles.Find(v => v.Id == id).FirstOrDefaultAsync();

        public async Task<List<Vehicle>> GetByUserIdAsync(string userId, int page = 1, int limit = 10) =>
            await _vehicles.Find(v => v.UserId == userId)
                           .Skip((page - 1) * limit)
                           .Limit(limit)
                           .ToListAsync();

        public async Task CreateAsync(Vehicle vehicle) =>
            await _vehicles.InsertOneAsync(vehicle);

        public async Task<bool> UpdateAsync(string id, Vehicle updated)
        {
            var result = await _vehicles.ReplaceOneAsync(v => v.Id == id, updated);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var result = await _vehicles.DeleteOneAsync(v => v.Id == id);
            return result.DeletedCount > 0;
        }

        // ================= Verify Services =================
        public async Task<bool> VerifyUserAsync(string userId)
        {
            var token = GetToken();
            if (string.IsNullOrEmpty(token))
            {
                Console.WriteLine("[VerifyUserAsync] No token found in request headers.");
                return false;
            }

            Console.WriteLine($"[VerifyUserAsync] Attempting to verify userId='{userId}' with token='{token.Substring(0, Math.Min(token.Length, 10))}...'");

            try
            {
                string endpointId = userId;

                if (ObjectId.TryParse(userId, out var objId))
                {
                    endpointId = objId.ToString();
                    Console.WriteLine($"[VerifyUserAsync] Parsed userId to ObjectId='{endpointId}'");
                }

                var request = new HttpRequestMessage(HttpMethod.Get, $"{_userServiceUrl}/api/User/{endpointId}");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                Console.WriteLine($"[VerifyUserAsync] Sending GET request to '{_userServiceUrl}/api/User/{endpointId}'");

                var response = await _httpClient.SendAsync(request);

                Console.WriteLine($"[VerifyUserAsync] Response StatusCode: {response.StatusCode}");
                if (!response.IsSuccessStatusCode)
                {
                    var body = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[VerifyUserAsync] Response body: {body}");
                }

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[VerifyUserAsync] Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> VerifyCompanyAsync(string companyId)
        {
            var token = GetToken();
            if (string.IsNullOrEmpty(token))
            {
                Console.WriteLine("[VerifyCompanyAsync] No token found in request headers.");
                return false;
            }

            Console.WriteLine($"[VerifyCompanyAsync] Attempting to verify companyId='{companyId}' with token='{token.Substring(0, Math.Min(token.Length, 10))}...'");

            try
            {
                string endpointId = companyId;

                if (ObjectId.TryParse(companyId, out var objId))
                {
                    endpointId = objId.ToString();
                    Console.WriteLine($"[VerifyCompanyAsync] Parsed companyId to ObjectId='{endpointId}'");
                }

                var request = new HttpRequestMessage(HttpMethod.Get, $"{_companyServiceUrl}/api/Companies/{endpointId}");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                Console.WriteLine($"[VerifyCompanyAsync] Sending GET request to '{_companyServiceUrl}/api/Companies/{endpointId}'");

                var response = await _httpClient.SendAsync(request);

                Console.WriteLine($"[VerifyCompanyAsync] Response StatusCode: {response.StatusCode}");
                if (!response.IsSuccessStatusCode)
                {
                    var body = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[VerifyCompanyAsync] Response body: {body}");
                }

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[VerifyCompanyAsync] Exception: {ex.Message}");
                return false;
            }
        }


    }
}
