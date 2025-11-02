using System.Text.Json.Serialization;
using BookingService.Models.Enums;

namespace BookingService.Models.DTOs
{
    public class BookingUpdateDto
    {
        [JsonPropertyName("start_time")]
        public DateTime? StartTime { get; set; }

        [JsonPropertyName("end_time")]
        public DateTime? EndTime { get; set; }

        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("rate_type")]
        public ChargingRateType? RateType { get; set; }
    }
}
