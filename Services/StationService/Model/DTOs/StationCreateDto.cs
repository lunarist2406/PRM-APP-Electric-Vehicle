namespace StationService.DTOs
{
    public class StationCreateDto
    {
        public string Name { get; set; } = null!;
        public string Address { get; set; } = null!;
        public double? Latitude { get; set; }    // nullable ✅
        public double? Longitude { get; set; }
        public int PowerCapacity { get; set; }
        public double PricePerKwh { get; set; }
        public string? Status { get; set; }
    }
}
