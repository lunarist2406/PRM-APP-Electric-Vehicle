using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using BookingService.Models.DTOs;

namespace BookingService.External
{
    public class ChargingPointClient : ExternalApiClientBase
    {
        private readonly string _baseUrl;

        public ChargingPointClient(HttpClient httpClient, ILogger<ChargingPointClient> logger, IConfiguration config)
            : base(httpClient, logger)
        {
            _baseUrl = config["CHARGINGPOINT_API_URL"] ?? "";
        }

        public async Task<ChargingPointResponseDto?> GetChargingPointByIdAsync(string id, string token)
        {
            var url = $"{_baseUrl}/api/charging-points/{id}";
            return await GetAsync<ChargingPointResponseDto>(url, token);
        }
    }
}
