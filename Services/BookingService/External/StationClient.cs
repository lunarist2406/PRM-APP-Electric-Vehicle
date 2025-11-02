using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using BookingService.Models.DTOs;

namespace BookingService.External
{
    public class StationClient : ExternalApiClientBase
    {
        private readonly string _baseUrl;

        public StationClient(HttpClient httpClient, ILogger<StationClient> logger, IConfiguration config)
            : base(httpClient, logger)
        {
            _baseUrl = config["STATION_API_URL"] ?? "";
        }

        public async Task<StationResponseDto?> GetStationByIdAsync(string id, string token)
        {
            var url = $"{_baseUrl}/api/stations/{id}";
            return await GetAsync<StationResponseDto>(url, token);
        }
    }
}
