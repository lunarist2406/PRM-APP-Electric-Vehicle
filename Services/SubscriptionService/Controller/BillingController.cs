using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SubscriptionService.Service;

namespace SubscriptionService.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class BillingController : ControllerBase
    {
        private readonly BillingService _billingService;

        public BillingController(BillingService billingService)
        {
            _billingService = billingService;
        }

        // POST: api/billing/generate-monthly-bills
        [HttpPost("generate-monthly-bills")]
        public async Task<IActionResult> GenerateMonthlyBills()
        {
            var results = await _billingService.GenerateMonthlyBillsForAllSubscriptions();
            return Ok(new { 
                message = "Monthly bills generated", 
                count = results.Count, 
                results 
            });
        }

        // POST: api/billing/calculate
        [HttpPost("calculate")]
        public async Task<IActionResult> CalculateBilling([FromBody] CalculateBillingDto dto)
        {
            try
            {
                var result = await _billingService.CalculateMonthlyBilling(
                    dto.VehicleId,
                    dto.SubscriptionId,
                    dto.TotalKwh
                );
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }

    public class CalculateBillingDto
    {
        public string VehicleId { get; set; } = string.Empty;
        public string SubscriptionId { get; set; } = string.Empty;
        public decimal TotalKwh { get; set; }
    }
}

