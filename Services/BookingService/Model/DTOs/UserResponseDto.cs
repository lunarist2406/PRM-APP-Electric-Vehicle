using BookingService.Models.Enums;

namespace BookingService.Models.DTOs
{
    public class UserResponseDto
    {
        public string Id { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        // Map role từ số sang enum
        public BookingUserRole Role { get; set; }

        public string Status { get; set; } = "active";
    }
}
