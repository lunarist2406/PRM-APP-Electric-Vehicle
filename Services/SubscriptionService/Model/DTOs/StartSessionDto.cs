namespace SubscriptionService.Model.DTOs
{
    public class StartSessionDto
    {
        public string VehicleSubscriptionId { get; set; } = string.Empty;
        public string StationId { get; set; } = string.Empty;
        public int SpotId { get; set; }
        public int? BookingId { get; set; }
        public DateTime StartTime { get; set; } = DateTime.UtcNow;
    }
}

