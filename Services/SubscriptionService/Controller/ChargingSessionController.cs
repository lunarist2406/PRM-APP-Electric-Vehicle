using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SubscriptionService.Model.DTOs;
using SubscriptionService.Service;

namespace SubscriptionService.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ChargingSessionController : ControllerBase
    {
        private readonly SubscriptionDataService _subscriptionService;
        private readonly BillingService _billingService;

        public ChargingSessionController(
            SubscriptionDataService subscriptionService,
            BillingService billingService)
        {
            _subscriptionService = subscriptionService;
            _billingService = billingService;
        }

        [HttpPost("start")]
        public async Task<IActionResult> StartSession([FromBody] StartSessionDto dto)
        {
            var stationKwh = await _billingService.GetStationKwh(dto.StationId);
            var session = await _subscriptionService.CreateSession(
                dto.VehicleSubscriptionId, 
                dto.StationId, 
                dto.SpotId, 
                dto.BookingId
            );
            
            if (session == null)
                return BadRequest(new { message = "Failed to create session" });
            
            return Ok(session);
        }

        [HttpPost("end")]
        public async Task<IActionResult> EndSession([FromBody] EndSessionDto dto)
        {
            var session = await _billingService.EndSession(
                dto.SessionId, 
                dto.BatteryNeededKwh,
                dto.ActualKwh, // Pass actualKwh from FE
                dto.StationId,
                dto.StationKwh ?? 65 // Default 65 kWh if not provided
            );
            
            if (session == null)
                return BadRequest(new { message = "Failed to end session" });
            
            return Ok(session);
        }

        [HttpGet("subscription/{subscriptionId}")]
        public async Task<IActionResult> GetSessionsBySubscription(string subscriptionId)
        {
            var sessions = await _subscriptionService.GetSessionsBySubscriptionId(subscriptionId);
            return Ok(sessions);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetSessionById(string id)
        {
            var session = await _subscriptionService.GetSessionById(id);
            if (session == null) return NotFound();
            return Ok(session);
        }
    }
}

