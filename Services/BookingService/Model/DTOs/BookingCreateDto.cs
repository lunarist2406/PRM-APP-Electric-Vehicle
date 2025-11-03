using System.Text.Json.Serialization;
using BookingService.Models.Enums;

namespace BookingService.Models.DTOs
{
    public class BookingCreateDto
    {
        [JsonPropertyName("user_id")]
        public string UserId { get; set; } = string.Empty;

        [JsonPropertyName("station_id")]
        public string StationId { get; set; } = string.Empty;

        [JsonPropertyName("vehicle_id")]
        public string VehicleId { get; set; } = string.Empty;

        [JsonPropertyName("chargingPoint_id")]
        public string ChargingPointId { get; set; } = string.Empty;

        [JsonPropertyName("start_time")]
        public DateTime StartTime { get; set; }

        [JsonPropertyName("end_time")]
        public DateTime EndTime { get; set; }

        [JsonPropertyName("rate_type")]
        public ChargingRateType RateType { get; set; } = ChargingRateType.Standard;
    }
}
