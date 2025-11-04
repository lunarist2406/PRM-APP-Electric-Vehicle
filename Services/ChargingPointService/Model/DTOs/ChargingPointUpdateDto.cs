namespace ChargingPointService.Models.DTOs
{
    public class ChargingPointUpdateDto
    {
        public string PointName { get; set; } = string.Empty;
        public string StationId { get; set; } = string.Empty;
        public string Type { get; set; } = "online";
        public string Status { get; set; } = "available";
    }
}
