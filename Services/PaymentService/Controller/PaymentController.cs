using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using PaymentService.Services;
using PaymentService.Models;

namespace PaymentService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly PaymentServiceLayer _service;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;

        public PaymentController(PaymentServiceLayer service, IConfiguration configuration, IWebHostEnvironment environment)
        {
            _service = service;
            _configuration = configuration;
            _environment = environment;
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

        // PUT: api/payment/approve/{id}
        [HttpPut("approve/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ApprovePayment(string id)
        {
            await _service.ApprovePaymentAsync(id);
            return Ok(new { message = "Payment approved" });
        }

        // PUT: api/payment/cancel/{id}
        [HttpPut("cancel/{id}")]
        [Authorize]
        public async Task<IActionResult> CancelPayment(string id)
        {
            await _service.CancelPaymentAsync(id);
            return Ok(new { message = "Payment canceled" });
        }

        // GET: api/payment/return-vnpay
        // VNPay sẽ redirect về đây sau khi thanh toán
        [HttpGet("return-vnpay")]
        public async Task<IActionResult> ReturnVNPay()
        {
            // Parse all query parameters from request
            var vnpParams = Request.Query.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.ToString()
            );
            
            var result = await _service.ProcessVNPayCallbackAsync(vnpParams);
            
            // Khi test local, trả về JSON thay vì redirect
            if (_environment.IsDevelopment())
            {
                return Ok(new
                {
                    success = result.Success,
                    message = result.Message,
                    paymentId = result.PaymentId
                });
            }
            
            // Production: redirect về frontend
            var frontendUrl = Environment.GetEnvironmentVariable("FRONTEND_URL") 
                ?? _configuration["Frontend:BaseUrl"] 
                ?? "https://yourapp.com";
            
            if (result.Success)
            {
                return Redirect($"{frontendUrl}/payment/success?paymentId={result.PaymentId}");
            }
            else
            {
                return Redirect($"{frontendUrl}/payment/failed?message={Uri.EscapeDataString(result.Message)}");
            }
        }
    }
}
