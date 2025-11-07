public class BillingItemDto
{
    public string VehicleId { get; set; } = string.Empty;
    public string SubscriptionId { get; set; } = string.Empty;
    public decimal TotalKwh { get; set; }
    public decimal KwhPrice { get; set; }
    public decimal KwhAmount { get; set; }
    public decimal BaseAmount { get; set; }
    public decimal DiscountPercent { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal Subtotal { get; set; }
    public decimal TotalAmount { get; set; }
}

public class BillingResponseDto
{
    public string Message { get; set; } = string.Empty; // fix warning
    public int Count { get; set; }
    public List<BillingItemDto> Results { get; set; } = new List<BillingItemDto>(); // fix warning
}
