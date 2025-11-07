using System;

namespace PaymentService.Models.DTOs
{
    public class PaymentResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? PaymentUrl { get; set; }
    }
}
