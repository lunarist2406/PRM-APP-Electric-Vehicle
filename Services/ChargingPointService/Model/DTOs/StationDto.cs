namespace ChargingPointService.Models.DTOs
{
    public class StationResponseDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int Power_Capacity { get; set; }
        public string Price_Per_KWH { get; set; } = string.Empty;
        public string Status { get; set; } = "available";
        public DateTime CreateAt { get; set; } = DateTime.UtcNow;
    }
}
