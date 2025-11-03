using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SubscriptionService.Service;

namespace SubscriptionService.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SubscriptionPlanController : ControllerBase
    {
        private readonly SubscriptionDataService _service;

        public SubscriptionPlanController(SubscriptionDataService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllPlans()
        {
            var plans = await _service.GetAllPlans();
            return Ok(plans);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPlanById(string id)
        {
            var plan = await _service.GetPlanById(id);
            if (plan == null) return NotFound();
            return Ok(plan);
        }

        [HttpPost]
        public async Task<IActionResult> CreatePlan([FromBody] SubscriptionService.Model.SubscriptionPlan plan)
        {
            await _service.CreatePlan(plan);
            return CreatedAtAction(nameof(GetPlanById), new { id = plan.Id }, plan);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePlan(string id, [FromBody] SubscriptionService.Model.SubscriptionPlan plan)
        {
            plan.Id = id;
            var updated = await _service.UpdatePlan(id, plan);
            if (!updated) return NotFound();
            return Ok(plan);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePlan(string id)
        {
            var deleted = await _service.DeletePlan(id);
            if (!deleted) return NotFound();
            return Ok(new { message = "Subscription plan deleted" });
        }
    }
}

