namespace SubscriptionService.Model.DTOs
{
    public class EndSessionDto
    {
        public string SessionId { get; set; } = string.Empty;
        public DateTime EndTime { get; set; } = DateTime.UtcNow;
        public decimal BatteryNeededKwh { get; set; } // kWh cần để sạc đầy pin (VD: xe 40% → cần 60% = 60 kWh)
        public decimal? ActualKwh { get; set; } // kWh thực tế đã sạc từ thiết bị (có thể < batteryNeededKwh nếu rút sớm)
        public string StationId { get; set; } = string.Empty;
        public decimal? StationKwh { get; set; }
    }
}

