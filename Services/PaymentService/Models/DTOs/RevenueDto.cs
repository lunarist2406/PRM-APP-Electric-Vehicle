using System;

namespace PaymentService.Models.DTOs
{
    public class RevenueDto
    {
        public decimal TotalRevenue { get; set; }
        public int TotalPaidPayments { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}

