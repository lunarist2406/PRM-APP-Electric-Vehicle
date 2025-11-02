namespace BookingService.Models.DTOs
{
    public class ChargingPointResponseDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Status { get; set; } = "available";
        public double PowerRating { get; set; }
    }
}
