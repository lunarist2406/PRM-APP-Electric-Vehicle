using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using BookingService.Models.DTOs;

namespace PaymentService.External
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
            var url = $"{_baseUrl}/api/Vehicles/{id}";
            return await GetAsync<VehicleResponseDto>(url, token);
        }

        public async Task<bool> UpdatePaymentStatusAsync(string vehicleId, string subscriptionId, string status, string token)
        {
            try
            {
                // Get current payment for this vehicle and subscription
                var currentPaymentUrl = $"{_baseUrl}/api/payment/current/{vehicleId}/{subscriptionId}";
                var currentPayment = await GetAsync<Dictionary<string, object>>(currentPaymentUrl, token);
                
                if (currentPayment == null || !currentPayment.ContainsKey("id"))
                {
                    _logger.LogWarning("Current payment not found for vehicle {VehicleId}, subscription {SubscriptionId}", vehicleId, subscriptionId);
                    return false;
                }

                var paymentId = currentPayment["id"]?.ToString();
                if (string.IsNullOrEmpty(paymentId))
                {
                    _logger.LogWarning("Payment ID is null for vehicle {VehicleId}", vehicleId);
                    return false;
                }

                // Update payment status
                var updateUrl = $"{_baseUrl}/api/payment/{paymentId}/status";
                var request = new HttpRequestMessage(HttpMethod.Patch, updateUrl);
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                request.Content = System.Net.Http.Json.JsonContent.Create(new { status = status });

                var response = await _httpClient.SendAsync(request);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Error updating payment status: {StatusCode} - {Error}", response.StatusCode, errorContent);
                    return false;
                }

                _logger.LogInformation("Successfully updated payment {PaymentId} status to {Status}", paymentId, status);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating payment status for vehicle {VehicleId}", vehicleId);
                return false;
            }
        }
    }
}
