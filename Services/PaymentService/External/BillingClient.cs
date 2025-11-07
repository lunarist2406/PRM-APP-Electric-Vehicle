using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PaymentService.Models.DTOs;

namespace PaymentService.External
{
    public class BillingClient : ExternalApiClientBase
    {
        private readonly string _baseUrl;

        public BillingClient(HttpClient httpClient, ILogger<BillingClient> logger, IConfiguration config)
            : base(httpClient, logger)
        {
            _baseUrl = config["BILLING_API_URL"] ?? string.Empty;
        }

        public async Task<BillingResponseDto?> GenerateMonthlyBillsAsync(string token)
        {
            var url = $"{_baseUrl}/api/billing/generate-monthly-bills";

            try
            {
                _logger.LogInformation("Calling BillingService: {Url}", url);
                var result = await PostAsync<BillingResponseDto>(url, null, token);
                _logger.LogInformation("BillingService response: {Result}", result != null ? "Success" : "Null");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling GenerateMonthlyBillsAsync: {Message}", ex.Message);
                return null;
            }
        }
    }
}
