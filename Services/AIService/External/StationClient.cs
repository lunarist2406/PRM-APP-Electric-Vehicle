using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using AIService.Models.DTOs;

namespace AIService.External
{
    public class StationClient : ExternalApiClientBase
    {
        private readonly string _baseUrl;

        public StationClient(HttpClient httpClient, ILogger<StationClient> logger, IConfiguration config)
            : base(httpClient, logger)
        {
            _baseUrl = config["STATION_API_URL"]
                       ?? throw new ArgumentNullException(nameof(config), "Missing STATION_API_URL in environment!");
        }

        // Lấy 1 trạm theo id, trả về object rỗng nếu null
        public async Task<StationResponseDto> GetStationByIdAsync(string id, string token)
        {
            var url = $"{_baseUrl}/api/Stations/{id}";
            var station = await GetAsync<StationResponseDto>(url, token);
            return station ?? new StationResponseDto(); // tránh null
        }

        // Lấy tất cả trạm, token cần truyền vào
        public async Task<List<StationResponseDto>> GetStationsAsync(string token)
        {
            var url = $"{_baseUrl}/api/Stations";
            var stations = await GetAsync<List<StationResponseDto>>(url, token);
            return stations ?? new List<StationResponseDto>(); // tránh null
        }
    }
}
