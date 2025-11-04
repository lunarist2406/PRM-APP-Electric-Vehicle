namespace SubscriptionService.Model.DTOs
{
    public class CalculateSessionCostDto
    {
        public decimal KwhUsed { get; set; }
        public decimal BatteryNeededKwh { get; set; }
        public decimal KwhPrice { get; set; }
        
        public decimal CalculateCost()
        {
            var energyToCharge = KwhUsed <= BatteryNeededKwh ? KwhUsed : BatteryNeededKwh;
            return energyToCharge * KwhPrice;
        }
    }
}

