using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StationService.DTOs;
using StationService.Models;
using StationService.Services;

namespace StationService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StationsController : ControllerBase
    {
        private readonly StationService.Services.StationService _stationService;

        public StationsController(StationService.Services.StationService stationService)
        {
            _stationService = stationService;
        }
        // 🔵 GET /api/stations/{id}
        [Authorize]
        [HttpGet("{id:length(24)}")]
        public async Task<IActionResult> GetById(string id)
        {
            var station = await _stationService.GetByIdAsync(id);
            if (station == null)
                return NotFound(new { message = "Station not found" });

            var response = new StationResponseDto
            {
                Id = station.Id,
                Name = station.Name,
                Address = station.Address,
                Latitude = station.Latitude,
                Longitude = station.Longitude,
                PowerCapacity = station.PowerCapacity,
                PricePerKwh = station.PricePerKwh,
                Status = station.Status,
                CreatedAt = station.CreatedAt
            };

            return Ok(response);
        }

        // 🟣 GET /api/stations
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var stations = await _stationService.GetAllAsync();
            var response = stations.Select(s => new StationResponseDto
            {
                Id = s.Id,
                Name = s.Name,
                Address = s.Address,
                Latitude = s.Latitude,
                Longitude = s.Longitude,
                PowerCapacity = s.PowerCapacity,
                PricePerKwh = s.PricePerKwh,
                Status = s.Status,
                CreatedAt = s.CreatedAt
            });
            return Ok(response);
        }
        // 🟢 POST /api/stations
        // 👉 Chỉ Staff hoặc Admin mới được tạo
        [Authorize(Roles = "Admin,Staff")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] StationCreateDto dto)
        {
            try
            {
                var station = new Station
                {
                    Name = dto.Name,
                    Address = dto.Address,
                    Latitude = dto.Latitude,
                    Longitude = dto.Longitude,
                    PowerCapacity = dto.PowerCapacity,
                    PricePerKwh = dto.PricePerKwh,
                    Status = dto.Status
                };

                var created = await _stationService.CreateAsync(station);

                var response = new StationResponseDto
                {
                    Id = created.Id,
                    Name = created.Name,
                    Address = created.Address,
                    Latitude = created.Latitude,
                    Longitude = created.Longitude,
                    PowerCapacity = created.PowerCapacity,
                    PricePerKwh = created.PricePerKwh,
                    Status = created.Status,
                    CreatedAt = created.CreatedAt
                };

                return CreatedAtAction(nameof(GetById), new { id = created.Id }, response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }



        // 🟡 PUT /api/stations/{id}
        [Authorize(Roles = "Admin,Staff")]
        [HttpPut("{id:length(24)}")]
        public async Task<IActionResult> Update(string id, [FromBody] StationUpdateDto dto)
        {
            var updated = await _stationService.UpdateAsync(id, dto);
            if (updated == null)
                return NotFound(new { message = "Station not found" });

            var response = new StationResponseDto
            {
                Id = updated.Id,
                Name = updated.Name,
                Address = updated.Address,
                Latitude = updated.Latitude,
                Longitude = updated.Longitude,
                PowerCapacity = updated.PowerCapacity,
                PricePerKwh = updated.PricePerKwh,
                Status = updated.Status,
                CreatedAt = updated.CreatedAt
            };

            return Ok(response);
        }

        // 🔴 DELETE /api/stations/{id}
        [Authorize(Roles = "Admin,Staff")]
        [HttpDelete("{id:length(24)}")]
        public async Task<IActionResult> Delete(string id)
        {
            var deleted = await _stationService.DeleteAsync(id);
            if (!deleted)
                return NotFound(new { message = "Station not found" });

            return NoContent();
        }
    }
}
