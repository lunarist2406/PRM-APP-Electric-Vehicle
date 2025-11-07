namespace PaymentService.Models.DTOs
{
    public class UserResponseDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public int? Role { get; set; } // Role từ UserService là số (0, 1, 2...)
        public string Status { get; set; } = "active";
    }
}
