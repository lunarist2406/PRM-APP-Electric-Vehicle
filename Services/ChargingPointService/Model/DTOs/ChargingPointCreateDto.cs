namespace ChargingPointService.Models.DTOs
{
    public class ChargingPointCreateDto
    {
        public string PointName { get; set; } = string.Empty;
        public string StationId { get; set; } = string.Empty;
        public string Type { get; set; } = "online";
        public string Status { get; set; } = "available";
    }
}
