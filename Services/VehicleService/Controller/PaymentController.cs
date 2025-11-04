using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehicleService.Models;
using VehicleService.Services;

namespace VehicleService.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PaymentController : ControllerBase
    {
        private readonly PaymentService _paymentService;

        public PaymentController(PaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        // POST: api/payment/add-kwh
        // Called by FE after charging session completes
        [HttpPost("add-kwh")]
        public async Task<IActionResult> AddKwh([FromBody] AddKwhDto dto)
        {
            var result = await _paymentService.AddKwhToPayment(
                dto.VehicleId,
                dto.SubscriptionId,
                dto.Kwh
            );

            if (!result)
                return BadRequest(new { message = "Failed to add kWh to payment" });

            return Ok(new { message = "kWh added successfully" });
        }

        // GET: api/payment/vehicle/{vehicleId}
        [HttpGet("vehicle/{vehicleId}")]
        public async Task<IActionResult> GetPaymentsByVehicle(string vehicleId)
        {
            var payments = await _paymentService.GetPaymentsByVehicleId(vehicleId);
            return Ok(payments);
        }

        // GET: api/payment/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPaymentById(string id)
        {
            var payment = await _paymentService.GetPaymentById(id);
            if (payment == null) return NotFound();
            return Ok(payment);
        }

        // GET: api/payment/current/{vehicleId}/{subscriptionId}
        [HttpGet("current/{vehicleId}/{subscriptionId}")]
        public async Task<IActionResult> GetCurrentPayment(string vehicleId, string subscriptionId)
        {
            var payment = await _paymentService.GetCurrentPayment(vehicleId, subscriptionId);
            if (payment == null) return NotFound();
            return Ok(payment);
        }

        // PATCH: api/payment/{id}/status
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdatePaymentStatus(string id, [FromBody] UpdatePaymentStatusDto dto)
        {
            var result = await _paymentService.UpdatePaymentStatus(id, dto.Status);
            if (!result) return NotFound();
            return Ok(new { message = "Payment status updated" });
        }

        // GET: api/payment/monthly-billing
        [HttpGet("monthly-billing")]
        public async Task<IActionResult> GetPaymentsForMonthlyBilling()
        {
            var payments = await _paymentService.GetPaymentsForMonthlyBilling();
            return Ok(payments);
        }

        // PATCH: api/payment/{id}/amounts
        [HttpPatch("{id}/amounts")]
        public async Task<IActionResult> UpdatePaymentAmounts(string id, [FromBody] UpdatePaymentAmountsDto dto)
        {
            var result = await _paymentService.UpdatePaymentAmounts(
                id, 
                dto.KwhAmount, 
                dto.BaseAmount, 
                dto.TotalAmount,
                dto.DiscountAmount,
                dto.Subtotal
            );
            
            if (!result) return NotFound();
            return Ok(new { message = "Payment amounts updated" });
        }
    }

    public class AddKwhDto
    {
        public string VehicleId { get; set; } = string.Empty;
        public string SubscriptionId { get; set; } = string.Empty;
        public decimal Kwh { get; set; }
    }

    public class UpdatePaymentStatusDto
    {
        public string Status { get; set; } = string.Empty;
    }

    public class UpdatePaymentAmountsDto
    {
        public decimal KwhAmount { get; set; }
        public decimal BaseAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal DiscountAmount { get; set; } = 0;
        public decimal Subtotal { get; set; } = 0;
    }
}

