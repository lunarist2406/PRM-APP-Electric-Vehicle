using System.Text.Json.Serialization;
using PaymentService.Models.Enums;

namespace PaymentService.Models.DTOs
{
    public class UserResponseDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public BookingUserRole Role { get; set; }
        public string Status { get; set; } = "active";
    }
}
