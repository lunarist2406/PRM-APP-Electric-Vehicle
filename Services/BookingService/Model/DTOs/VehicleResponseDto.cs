namespace BookingService.Models.DTOs
{
    public class VehicleResponseDto
    {
        public string Id { get; set; } = string.Empty;
        public string LicensePlate { get; set; } = string.Empty; // maps from plate_number
        public string Model { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string? CompanyId { get; set; }
        public double BatteryCapacity { get; set; }
    }
}
