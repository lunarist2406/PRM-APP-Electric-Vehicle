using System.Threading.Tasks;
using BookingService.Models.DTOs;

namespace BookingService.External
{
    public class ChargingPointClient : ExternalApiClientBase
    {
        private readonly string _baseUrl;

        public ChargingPointClient(HttpClient httpClient, ILogger<ChargingPointClient> logger, IConfiguration config)
            : base(httpClient, logger)
        {
            _baseUrl = config["CHARGINGPOINT_API_URL"] ?? string.Empty;
        }

        public async Task<ChargingPointResponseDto?> GetChargingPointByIdAsync(string id, string token)
        {
            if (string.IsNullOrEmpty(id))
                return null;

            var url = $"{_baseUrl}/api/ChargingPoint/{id}";

            try
            {
                var response = await GetAsync<ApiResponse<ChargingPointResponseDto>>(url, token);
                return response?.Data;
            }
            catch
            {
                return null;
            }
        }
    }
}
