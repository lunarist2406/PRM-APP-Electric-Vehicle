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
            var url = $"{_baseUrl}/api/Billing/generate-monthly-bills";

            try
            {
                return await PostAsync<BillingResponseDto>(url, null, token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling GenerateMonthlyBillsAsync");
                return null;
            }
        }
    }
}
