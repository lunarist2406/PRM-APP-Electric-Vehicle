using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaymentService.Services;
using PaymentService.Models;

namespace PaymentService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly PaymentServiceLayer _service;

        public PaymentController(PaymentServiceLayer service)
        {
            _service = service;
        }

        // POST: api/payment
        [HttpPost("generate")]
        [Authorize]
        public async Task<IActionResult> GeneratePayments()
        {
            var authHeader = Request.Headers["Authorization"].ToString();
            var token = authHeader.StartsWith("Bearer ") ? authHeader.Substring("Bearer ".Length) : authHeader;

            var result = await _service.CreatePaymentsFromBillingAsync(token);

            if (!result.Success)
                return Ok(new { success = false, message = result.Message });

            return Ok(new
            {
                success = true,
                message = result.Message,
                paymentUrl = result.PaymentUrl
            });
        }



        // GET: api/payment/my
        [HttpGet("my")]
        [Authorize]
        public async Task<IActionResult> GetMyPayments()
        {
            var userId = User.FindFirst("sub")?.Value;
            var payments = await _service.GetUserPaymentsAsync(userId!);
            return Ok(payments);
        }

        // GET: api/payment/all
        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllPayments()
        {
            var payments = await _service.GetAllPaymentsAsync();
            return Ok(payments);
        }

<<<<<<< Updated upstream
        // PUT: api/payment/approve/{id}
        [HttpPut("approve/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ApprovePayment(string id)
        {
            await _service.ApprovePaymentAsync(id);
            return Ok(new { message = "Payment approved" });
=======
        // GET: api/payment/revenue
        [HttpGet("revenue")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetRevenue([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            var revenue = await _service.GetRevenueAsync(startDate, endDate);
            return Ok(revenue);
>>>>>>> Stashed changes
        }

        // PUT: api/payment/cancel/{id}
        [HttpPut("cancel/{id}")]
        [Authorize]
        public async Task<IActionResult> CancelPayment(string id)
        {
            await _service.CancelPaymentAsync(id);
            return Ok(new { message = "Payment canceled" });
        }
    }
}
