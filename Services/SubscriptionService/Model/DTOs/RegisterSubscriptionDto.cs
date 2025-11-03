namespace SubscriptionService.Model.DTOs
{
    public class RegisterSubscriptionDto
    {
        public string VehicleId { get; set; } = string.Empty;
        public string SubscriptionId { get; set; } = string.Empty;
        public bool AutoRenew { get; set; } = false;
    }
}

