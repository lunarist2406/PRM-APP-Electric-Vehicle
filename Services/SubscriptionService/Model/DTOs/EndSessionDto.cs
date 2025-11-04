namespace SubscriptionService.Model.DTOs
{
    public class EndSessionDto
    {
        public string SessionId { get; set; } = string.Empty;
        public DateTime EndTime { get; set; } = DateTime.UtcNow;
        public decimal BatteryNeededKwh { get; set; }
        public string StationId { get; set; } = string.Empty;
        public decimal? StationKwh { get; set; }
    }
}

