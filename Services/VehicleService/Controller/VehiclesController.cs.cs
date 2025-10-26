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
			var userIdFromToken = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (string.IsNullOrEmpty(userIdFromToken))
				return Unauthorized(new { message = "Access token required" });

			string? targetUserId = null;
			string? targetCompanyId = null;

			// ✅ Check user_id từ DTO trước
			if (!string.IsNullOrEmpty(dto.UserId))
			{
				var userValid = await _vehicleService.VerifyUserAsync(dto.UserId);
				if (userValid)
				{
					targetUserId = dto.UserId;
				}
				else
				{
					// Nếu user không tồn tại, targetUserId = null
					Console.WriteLine($"[Create] User {dto.UserId} not found, setting null");
				}
			}

			// Nếu DTO không có userId, fallback sang token
			if (targetUserId == null)
			{
				var tokenUserValid = await _vehicleService.VerifyUserAsync(userIdFromToken);
				if (tokenUserValid)
					targetUserId = userIdFromToken;
				else
					targetUserId = null; // token cũng invalid -> để null
			}

			// ✅ Check company_id từ DTO
			if (!string.IsNullOrEmpty(dto.CompanyId))
			{
				var companyValid = await _vehicleService.VerifyCompanyAsync(dto.CompanyId);
				if (companyValid)
				{
					targetCompanyId = dto.CompanyId;
				}
				else
				{
					Console.WriteLine($"[Create] Company {dto.CompanyId} not found, setting null");
					targetCompanyId = null;
				}
			}

			var vehicle = new Vehicle
			{
				UserId = targetUserId,
				CompanyId = targetCompanyId,
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
			if (existing == null)
				return NotFound(new { message = "Vehicle not found" });

			// ✅ Chỉ cập nhật UserId nếu DTO có, không fallback token
			existing.UserId = !string.IsNullOrEmpty(dto.UserId)
				? dto.UserId
				: null;

			// ✅ Chỉ cập nhật CompanyId nếu DTO có, không fallback token
			existing.CompanyId = !string.IsNullOrEmpty(dto.CompanyId)
				? dto.CompanyId
				: null;

			existing.Model = dto.Model;
			existing.PlateNumber = dto.PlateNumber;
			existing.BatteryCapacity = dto.BatteryCapacity;
			existing.UpdatedAt = DateTime.UtcNow;

			var success = await _vehicleService.UpdateAsync(id, existing);
			if (!success)
				return StatusCode(500, new { message = "Failed to update vehicle" });

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
