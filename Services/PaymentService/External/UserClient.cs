using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PaymentService.Models.DTOs;

namespace PaymentService.External
{
    public class UserClient : ExternalApiClientBase
    {
        private readonly string _baseUrl;

        public UserClient(HttpClient httpClient, ILogger<UserClient> logger, IConfiguration config)
            : base(httpClient, logger)
        {
            _baseUrl = config["USER_API_URL"] ?? "";
        }

        public async Task<UserResponseDto?> GetUserByIdAsync(string id, string token)
        {
            var url = $"{_baseUrl}/api/User/{id}";
            return await GetAsync<UserResponseDto>(url, token);
        }
    }
}
