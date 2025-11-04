using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ChargingPointService.Models.DTOs;
using ChargingPointService.Services;

namespace ChargingPointService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ChargingPointController : ControllerBase
    {
        private readonly ChargingPointApiService _service;
        private readonly ILogger<ChargingPointController> _logger;

        public ChargingPointController(ChargingPointApiService service, ILogger<ChargingPointController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var list = await _service.GetAllAsync();
            return Ok(new { message = "Successfully fetched all charging points.", data = list });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var item = await _service.GetByIdAsync(id);
            if (item == null)
                return NotFound(new { message = $" ChargingPoint with ID {id} not found." });

            return Ok(new { message = " Found charging point.", data = item });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ChargingPointCreateDto dto)
        {
            var created = await _service.CreateAsync(dto);

            if (created == null)
            {
                // Trường hợp station không tồn tại hoặc token lỗi
                return BadRequest(new
                {
                    message = $"Failed to create charging point. Station {dto.StationId} may not exist or token invalid."
                });
            }

            return CreatedAtAction(nameof(GetById), new { id = created.Id }, new
            {
                message = "Charging point created successfully.",
                data = created
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] ChargingPointUpdateDto dto)
        {
            var result = await _service.UpdateAsync(id, dto);

            if (result == null)
            {
                // Chia nhỏ message: kiểm tra trước ChargingPoint, sau đó station
                var existsPoint = await _service.GetByIdAsync(id);
                if (existsPoint == null)
                {
                    return NotFound(new { message = $"ChargingPoint with ID {id} not found." });
                }
                else if (!string.IsNullOrEmpty(dto.StationId))
                {
                    return NotFound(new { message = $"Station with ID {dto.StationId} not found or unreachable." });
                }
                else
                {
                    return BadRequest(new { message = $"Failed to update ChargingPoint with ID {id}." });
                }
            }

            return Ok(new
            {
                message = "Charging point updated successfully.",
                data = result
            });
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var success = await _service.DeleteAsync(id);
            if (!success)
                return NotFound(new { message = $" ChargingPoint with ID {id} not found or already deleted." });

            return Ok(new { message = " Charging point deleted successfully." });
        }
    }
}
