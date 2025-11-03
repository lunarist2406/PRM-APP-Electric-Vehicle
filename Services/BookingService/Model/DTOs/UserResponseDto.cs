using System.Text.Json.Serialization;
using BookingService.Models.Enums;

namespace BookingService.Models.DTOs
{
    public class UserResponseDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        [JsonConverter(typeof(JsonStringEnumConverter))] // ✅ Tự convert giữa string/int ↔ enum
        public BookingUserRole Role { get; set; }

        public string Status { get; set; } = "active";
    }
}
