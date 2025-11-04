using BookingService.Models;
using BookingService.Models.DTOs;
using BookingService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
namespace BookingService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class BookingsController : ControllerBase
    {
        private readonly BookingServiceLayer _service;

        public BookingsController(BookingServiceLayer service)
        {
            _service = service;
        }

        private string? GetToken() =>
            HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

        // ==========================================
        // 📘 GET: Danh sách booking (có filter)
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? user_id,
            [FromQuery] string? station_id,
            [FromQuery] string? vehicle_id,
            [FromQuery] string? chargingPoint_id,
            [FromQuery] string? status,
            [FromQuery] DateTime? start_date,
            [FromQuery] DateTime? end_date,
            [FromQuery] int page = 1,
            [FromQuery] int limit = 10)
        {
            var token = GetToken();
            if (string.IsNullOrEmpty(token))
                return Unauthorized(new { message = "Access token required" });

            var result = await _service.GetAllAsync(
                user_id, station_id, vehicle_id, chargingPoint_id,
                status, start_date, end_date, page, limit, token);

            return Ok(result);
        }

        // ==========================================
        // 📘 GET: Lấy booking theo ID
        // ==========================================
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var token = GetToken();
            if (string.IsNullOrEmpty(token))
                return Unauthorized(new { message = "Access token required" });

            var booking = await _service.GetByIdAsync(id, token);
            if (booking == null)
                return NotFound(new { message = "Booking not found" });

            return Ok(booking);
        }

        // ==========================================
        // 🟢 POST: Tạo booking mới
        // ==========================================
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] BookingCreateDto dto)
        {
            var token = GetToken();
            if (string.IsNullOrEmpty(token))
                return Unauthorized(new { message = "Access token required" });

            try
            {
                var created = await _service.CreateAsync(dto, token);
                if (created == null)
                    return BadRequest(new { message = "Failed to create booking." });

                return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ==========================================
        // 🟡 PUT: Cập nhật booking
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] BookingUpdateDto dto)
        {
            var updated = await _service.UpdateAsync(id, dto);
            if (!updated)
                return NotFound(new { message = "Booking not found" });

            return Ok(new { message = "Booking updated successfully" });
        }


        // ==========================================
        // 🔴 DELETE: Xoá booking
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var deleted = await _service.DeleteAsync(id);
            if (!deleted)
                return NotFound(new { message = "Booking not found" });

            return Ok(new { message = "Booking deleted successfully" });
        }

        // ==========================================
        // 🧍 GET: Lấy booking của user hiện tại
        // ==========================================
        [HttpGet("me")]
        public async Task<IActionResult> GetMyBookings()
        {
            var token = GetToken();
            if (string.IsNullOrEmpty(token))
                return Unauthorized(new { message = "Access token required" });

            // 👇 Lấy userId từ JWT claim "sub" hoặc "user_id"
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return BadRequest(new { message = "User ID not found in token." });

            var bookings = await _service.GetMyBookingsAsync(userId, token);
            return Ok(bookings);
        }

        // ==========================================
        // 🏢 GET: Lấy booking theo trạm (stationId)
        // ==========================================
        [HttpGet("station/{stationId}")]
        public async Task<IActionResult> GetByStation(string stationId)
        {
            var token = GetToken();
            if (string.IsNullOrEmpty(token))
                return Unauthorized(new { message = "Access token required" });

            var bookings = await _service.GetByStationIdAsync(stationId, token);
            return Ok(bookings);
        }

        // ==========================================
        // ❌ PATCH: Hủy booking
        // ==========================================
        [HttpPatch("{id}/cancel")]
        public async Task<IActionResult> Cancel(string id)
        {
            try
            {
                var cancelled = await _service.CancelBookingAsync(id);
                if (!cancelled)
                    return NotFound(new { message = "Booking not found" });

                return Ok(new { message = "Booking cancelled successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ==========================================
        // ✅ PATCH: Xác nhận booking
        // ==========================================
        [HttpPatch("{id}/confirm")]
        public async Task<IActionResult> Confirm(string id)
        {
            try
            {
                var confirmed = await _service.ConfirmBookingAsync(id);
                if (!confirmed)
                    return NotFound(new { message = "Booking not found" });

                return Ok(new { message = "Booking confirmed successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
