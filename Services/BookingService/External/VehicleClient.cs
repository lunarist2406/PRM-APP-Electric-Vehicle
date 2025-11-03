using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using BookingService.Models.DTOs;

namespace BookingService.External
{
    public class VehicleClient : ExternalApiClientBase
    {
        private readonly string _baseUrl;

        public VehicleClient(HttpClient httpClient, ILogger<VehicleClient> logger, IConfiguration config)
            : base(httpClient, logger)
        {
            _baseUrl = config["VEHICLE_API_URL"] ?? "";
        }

        public async Task<VehicleResponseDto?> GetVehicleByIdAsync(string id, string token)
        {
            var url = $"{_baseUrl}/api/vehicles/{id}";
            return await GetAsync<VehicleResponseDto>(url, token);
        }
    }
}
