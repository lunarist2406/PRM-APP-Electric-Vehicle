namespace BookingService.Models.DTOs
{
    public class StationResponseDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int PowerCapacity { get; set; }
        public decimal PricePerKwh { get; set; }
        public string Status { get; set; } = "available";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
