namespace StationService.DTOs
{
    public class StationUpdateDto
    {
        public string? Name { get; set; }
        public string? Address { get; set; }
        public double? Latitude { get; set; }    // nullable ✅
        public double? Longitude { get; set; }
        public int? PowerCapacity { get; set; }
        public double? PricePerKwh { get; set; }
        public string? Status { get; set; }
    }
}
