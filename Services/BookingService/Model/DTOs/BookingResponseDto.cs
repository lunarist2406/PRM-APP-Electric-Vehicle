using System.Text.Json.Serialization;

namespace BookingService.Models.DTOs
{
    public class BookingResponseDto
    {
        [JsonPropertyName("_id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("user_id")]
        public UserResponseDto? User { get; set; }

        [JsonPropertyName("station_id")]
        public StationResponseDto? Station { get; set; }

        [JsonPropertyName("vehicle_id")]
        public VehicleResponseDto? Vehicle { get; set; }

        [JsonPropertyName("chargingPoint_id")]
        public ChargingPointResponseDto? ChargingPoint { get; set; }

        [JsonPropertyName("start_time")]
        public DateTime StartTime { get; set; }

        [JsonPropertyName("end_time")]
        public DateTime EndTime { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; } = "pending";

        [JsonPropertyName("rate_type")]
        public string RateType { get; set; } = "Standard";

        [JsonPropertyName("total_fee")]
        public double TotalFee { get; set; }

        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("updatedAt")]
        public DateTime UpdatedAt { get; set; }
    }
}
