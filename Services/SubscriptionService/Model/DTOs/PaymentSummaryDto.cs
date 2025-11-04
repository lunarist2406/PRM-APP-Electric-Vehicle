namespace SubscriptionService.Model.DTOs
{
    public class PaymentSummaryDto
    {
        public string VehicleSubscriptionId { get; set; } = string.Empty;
        public decimal TotalKwh { get; set; }
        public decimal TotalAmount { get; set; }
        public int TotalSessions { get; set; }
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public List<InvoiceDetailDto> Invoices { get; set; } = new();
    }

    public class InvoiceDetailDto
    {
        public string InvoiceId { get; set; } = string.Empty;
        public decimal TotalKwh { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime InvoiceDate { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}

