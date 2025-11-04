using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SubscriptionService.Model.DTOs;
using SubscriptionService.Service;

namespace SubscriptionService.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class VehicleSubscriptionController : ControllerBase
    {
        private readonly SubscriptionDataService _service;

        public VehicleSubscriptionController(SubscriptionDataService service)
        {
            _service = service;
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterSubscription([FromBody] RegisterSubscriptionDto dto)
        {
            try
            {
                var subscription = await _service.CreateVehicleSubscription(dto);
                if (subscription == null)
                    return BadRequest(new { message = "Failed to create subscription. Invalid plan or vehicle." });
                
                return Ok(subscription);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("vehicle/{vehicleId}")]
        public async Task<IActionResult> GetSubscriptionByVehicle(string vehicleId)
        {
            var subscription = await _service.GetActiveSubscriptionByVehicleId(vehicleId);
            if (subscription == null) return NotFound();
            return Ok(subscription);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetSubscriptionById(string id)
        {
            var subscription = await _service.GetVehicleSubscriptionById(id);
            if (subscription == null) return NotFound();
            return Ok(subscription);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllSubscriptions()
        {
            // Get user ID from token (would need to extract from JWT)
            var subscriptions = await _service.GetAllVehicleSubscriptions();
            return Ok(subscriptions);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSubscription(string id, [FromBody] UpdateVehicleSubscriptionDto dto)
        {
            var subscription = await _service.GetVehicleSubscriptionById(id);
            if (subscription == null) return NotFound();

            subscription.AutoRenew = dto.AutoRenew ?? subscription.AutoRenew;
            subscription.PaymentStatus = dto.PaymentStatus ?? subscription.PaymentStatus;
            subscription.UpdatedAt = DateTime.UtcNow;

            var result = await _service.UpdateVehicleSubscription(id, subscription);
            if (!result) return BadRequest();

            return Ok(subscription);
        }

        [HttpPatch("{id}/auto-renew")]
        public async Task<IActionResult> ToggleAutoRenew(string id)
        {
            var subscription = await _service.GetVehicleSubscriptionById(id);
            if (subscription == null) return NotFound();

            subscription.AutoRenew = !subscription.AutoRenew;
            subscription.UpdatedAt = DateTime.UtcNow;

            var result = await _service.UpdateVehicleSubscription(id, subscription);
            if (!result) return BadRequest();

            return Ok(subscription);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSubscription(string id)
        {
            var result = await _service.DeleteVehicleSubscription(id);
            if (!result) return NotFound();
            return Ok(new { message = "Subscription deleted" });
        }

        [HttpGet("check/{vehicleId}")]
        public async Task<IActionResult> CheckVehicleHasSubscription(string vehicleId)
        {
            var subscription = await _service.GetActiveSubscriptionByVehicleId(vehicleId);
            return Ok(new { hasSubscription = subscription != null, subscription });
        }
    }

    public class UpdateVehicleSubscriptionDto
    {
        public bool? AutoRenew { get; set; }
        public string? PaymentStatus { get; set; }
    }
}

