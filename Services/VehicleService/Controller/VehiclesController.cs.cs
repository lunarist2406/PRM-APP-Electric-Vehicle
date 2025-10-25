using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VehicleService.Models;
using VehicleService.Models.DTOs;
using VehicleService.Services;
using System.Security.Claims;

namespace VehicleService.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class VehiclesController : ControllerBase
	{
		private readonly VehicleDataService _vehicleService;

		public VehiclesController(VehicleDataService vehicleService)
		{
			_vehicleService = vehicleService;
		}

		[HttpGet]
		[Authorize]
		public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int limit = 10)
		{
			var vehicles = await _vehicleService.GetAllAsync(page, limit);
			return Ok(new
			{
				vehicles,
				pagination = new
				{
					currentPage = page,
					itemsPerPage = limit,
					totalItems = vehicles.Count,
					totalPages = (int)Math.Ceiling((double)vehicles.Count / limit)
				}
			});
		}

		[HttpGet("{id}")]
		[Authorize]
		public async Task<IActionResult> GetById(string id)
		{
			var vehicle = await _vehicleService.GetByIdAsync(id);
			if (vehicle == null) return NotFound(new { message = "Vehicle not found" });
			return Ok(vehicle);
		}

		[HttpGet("myVehicle")]
		[Authorize]
		public async Task<IActionResult> GetMyVehicles([FromQuery] int page = 1, [FromQuery] int limit = 10)
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (string.IsNullOrEmpty(userId))
				return Unauthorized(new { message = "Access token required" });

			var validUser = await _vehicleService.VerifyUserAsync(userId);
			if (!validUser)
				return BadRequest(new { message = "User does not exist in UserService" });

			var vehicles = await _vehicleService.GetByUserIdAsync(userId, page, limit);
			return Ok(new
			{
				vehicles,
				pagination = new
				{
					currentPage = page,
					itemsPerPage = limit,
					totalItems = vehicles.Count,
					totalPages = (int)Math.Ceiling((double)vehicles.Count / limit)
				}
			});
		}

		[HttpPost]
		[Authorize]
		public async Task<IActionResult> Create([FromBody] VehicleDto dto)
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (string.IsNullOrEmpty(userId))
				return Unauthorized(new { message = "Access token required" });

			var userValid = await _vehicleService.VerifyUserAsync(userId);
			if (!userValid) return BadRequest(new { message = "User not found in UserService" });

			if (!string.IsNullOrEmpty(dto.CompanyId))
			{
				var companyValid = await _vehicleService.VerifyCompanyAsync(dto.CompanyId);
				if (!companyValid) return BadRequest(new { message = "Company not found in CompanyService" });
			}

			var vehicle = new Vehicle
			{
				UserId = dto.UserId ?? userId,
				CompanyId = dto.CompanyId,
				PlateNumber = dto.PlateNumber,
				Model = dto.Model,
				BatteryCapacity = dto.BatteryCapacity,
				CreatedAt = DateTime.UtcNow,
				UpdatedAt = DateTime.UtcNow
			};

			await _vehicleService.CreateAsync(vehicle);
			return CreatedAtAction(nameof(GetById), new { id = vehicle.Id }, vehicle);
		}

		[HttpPut("{id}")]
		[Authorize]
		public async Task<IActionResult> Update(string id, [FromBody] VehicleDto dto)
		{
			var existing = await _vehicleService.GetByIdAsync(id);
			if (existing == null) return NotFound(new { message = "Vehicle not found" });

			existing.Model = dto.Model;
			existing.PlateNumber = dto.PlateNumber;
			existing.CompanyId = dto.CompanyId;
			existing.BatteryCapacity = dto.BatteryCapacity;
			existing.UpdatedAt = DateTime.UtcNow;

			var success = await _vehicleService.UpdateAsync(id, existing);
			if (!success) return StatusCode(500, new { message = "Failed to update vehicle" });

			return Ok(existing);
		}

		[HttpDelete("{id}")]
		[Authorize]
		public async Task<IActionResult> Delete(string id)
		{
			var success = await _vehicleService.DeleteAsync(id);
			if (!success) return NotFound(new { message = "Vehicle not found" });

			return Ok(new { message = "Vehicle deleted" });
		}
	}
}
