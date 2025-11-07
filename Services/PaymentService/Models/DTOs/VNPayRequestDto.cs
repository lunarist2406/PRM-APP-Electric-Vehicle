namespace PaymentService.Models.DTOs
{
    public class VNPayRequestDto
    {
        public string OrderId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string OrderInfo { get; set; } = string.Empty;
        public string? OrderType { get; set; }
        public string ReturnUrl { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
    }
}
