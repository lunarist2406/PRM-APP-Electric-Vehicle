using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace PaymentService.External
{
    public class ExternalApiClientBase
    {
        protected readonly HttpClient _httpClient;
        protected readonly ILogger _logger;

        public ExternalApiClientBase(HttpClient httpClient, ILogger logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        protected async Task<T?> GetAsync<T>(string url, string token)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(json);
        }

        protected async Task<T?> PostAsync<T>(string url, object? body, string? token = null)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, url);

                if (token != null)
                    request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                if (body != null)
                    request.Content = JsonContent.Create(body);

                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Billing API raw response: {json}", json);

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                return JsonSerializer.Deserialize<T>(json, options);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling external POST {url}", url);
                return default;
            }
        }
    }
}
