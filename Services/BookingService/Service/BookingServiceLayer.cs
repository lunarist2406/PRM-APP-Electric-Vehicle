using BookingService.Models;
using BookingService.Models.DTOs;
using BookingService.Repositories;
using BookingService.External;
using BookingService.Models.Enums;
using Microsoft.Extensions.Logging;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
namespace BookingService.Services
{
    public class BookingServiceLayer
    {
        private readonly IBookingRepository _repo;
        private readonly UserClient _userClient;
        private readonly VehicleClient _vehicleClient;
        private readonly StationClient _stationClient;
        private readonly ChargingPointClient _chargingPointClient;
        private readonly ILogger<BookingServiceLayer> _logger;

        public BookingServiceLayer(
            IBookingRepository repo,
            UserClient userClient,
            VehicleClient vehicleClient,
            StationClient stationClient,
            ChargingPointClient chargingPointClient,
            ILogger<BookingServiceLayer> logger)
        {
            _repo = repo;
            _userClient = userClient;
            _vehicleClient = vehicleClient;
            _stationClient = stationClient;
            _chargingPointClient = chargingPointClient;
            _logger = logger;
        }

        // ------------------- GET ALL -------------------
        public async Task<List<Booking>> GetAllAsync(
            string? userId, string? stationId, string? vehicleId, string? chargingPointId,
            string? status, DateTime? startDate, DateTime? endDate, int page, int limit,
            string token)
        {
            // Parse status string -> enum nếu có
            BookingStatus? parsedStatus = null;
            if (!string.IsNullOrEmpty(status) && Enum.TryParse(status, true, out BookingStatus s))
                parsedStatus = s;

            var bookings = await _repo.GetAllAsync(
                userId, stationId, vehicleId, chargingPointId,
                parsedStatus?.ToString(), startDate, endDate, page, limit);

            foreach (var b in bookings)
            {
                b.UserInfo = await _userClient.GetUserByIdAsync(b.UserId, token);
                b.VehicleInfo = await _vehicleClient.GetVehicleByIdAsync(b.VehicleId, token);
                b.StationInfo = await _stationClient.GetStationByIdAsync(b.StationId, token);
                b.ChargingPointInfo = await _chargingPointClient.GetChargingPointByIdAsync(b.ChargingPointId, token);
            }

            return bookings;
        }

        // ------------------- GET BY ID -------------------
        public async Task<Booking?> GetByIdAsync(string id, string token)
        {
            var booking = await _repo.GetByIdAsync(id);
            if (booking == null) return null;

            booking.UserInfo = await _userClient.GetUserByIdAsync(booking.UserId, token);
            booking.VehicleInfo = await _vehicleClient.GetVehicleByIdAsync(booking.VehicleId, token);
            booking.StationInfo = await _stationClient.GetStationByIdAsync(booking.StationId, token);
            booking.ChargingPointInfo = await _chargingPointClient.GetChargingPointByIdAsync(booking.ChargingPointId, token);

            return booking;
        }

        // ------------------- CREATE -------------------


public async Task<Booking?> CreateAsync(BookingCreateDto dto, string token)
    {
        // Nếu user_id không được gửi trong DTO, lấy từ token
        if (string.IsNullOrEmpty(dto.UserId))
        {
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token.Replace("Bearer ", ""));
            var userIdFromToken = jwt.Claims.FirstOrDefault(c =>
                c.Type == ClaimTypes.NameIdentifier || c.Type == "sub" || c.Type == "user_id"
            )?.Value;

            if (string.IsNullOrEmpty(userIdFromToken))
                throw new Exception("Không thể xác định user_id từ token.");

            dto.UserId = userIdFromToken;
        }

        // Kiểm tra user tồn tại
        var user = await _userClient.GetUserByIdAsync(dto.UserId, token);
        if (user == null)
            throw new Exception($"User {dto.UserId} không tồn tại.");

        // Kiểm tra station tồn tại
        var station = await _stationClient.GetStationByIdAsync(dto.StationId, token);
        if (station == null)
            throw new Exception($"Station {dto.StationId} không tồn tại.");

        // Kiểm tra vehicle tồn tại
        var vehicle = await _vehicleClient.GetVehicleByIdAsync(dto.VehicleId, token);
        if (vehicle == null)
            throw new Exception($"Vehicle {dto.VehicleId} không tồn tại.");

        // Kiểm tra charging point tồn tại
        var charging = await _chargingPointClient.GetChargingPointByIdAsync(dto.ChargingPointId, token);
        if (charging == null)
            throw new Exception($"Charging point {dto.ChargingPointId} không tồn tại.");

        // Tạo booking mới
        var booking = new Booking
        {
            UserId = dto.UserId,
            StationId = dto.StationId,
            VehicleId = dto.VehicleId,
            ChargingPointId = dto.ChargingPointId,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,
            RateType = dto.RateType,
            Status = BookingStatus.Pending
        };

        booking.CalculateTotalFee();
        await _repo.CreateAsync(booking);
        _logger.LogInformation("✅ Booking {Id} created successfully for user {User}", booking.Id, dto.UserId);

        return booking;
    }


    // ------------------- UPDATE -------------------
    public async Task<bool> UpdateAsync(string id, BookingUpdateDto dto)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) return false;

            // Update các trường có giá trị mới
            if (dto.StartTime.HasValue)
                existing.StartTime = dto.StartTime.Value;

            if (dto.EndTime.HasValue)
                existing.EndTime = dto.EndTime.Value;

            if (!string.IsNullOrEmpty(dto.Status))
            {
                if (Enum.TryParse(dto.Status, true, out BookingStatus parsedStatus))
                    existing.Status = parsedStatus;
                else
                    throw new Exception($"Trạng thái không hợp lệ: {dto.Status}");
            }

            if (dto.RateType.HasValue)
                existing.RateType = dto.RateType.Value;

            // Tự tính lại phí nếu có thay đổi thời gian hoặc rate type
            existing.CalculateTotalFee();
            existing.UpdatedAt = DateTime.UtcNow;

            await _repo.UpdateAsync(id, existing);
            return true;
        }


        // ------------------- DELETE -------------------
        public async Task<bool> DeleteAsync(string id)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) return false;

            await _repo.DeleteAsync(id);
            return true;
        }

        // ------------------- GET MY BOOKINGS -------------------
        public async Task<List<Booking>> GetMyBookingsAsync(string userId, string token)
        {
            var bookings = await _repo.GetAllAsync(userId, null, null, null, null, null, null, 1, 100);
            foreach (var b in bookings)
            {
                b.StationInfo = await _stationClient.GetStationByIdAsync(b.StationId, token);
                b.VehicleInfo = await _vehicleClient.GetVehicleByIdAsync(b.VehicleId, token);
            }
            return bookings;
        }

        // ------------------- GET BY STATION -------------------
        public async Task<List<Booking>> GetByStationIdAsync(string stationId, string token)
        {
            var bookings = await _repo.GetAllAsync(null, stationId, null, null, null, null, null, 1, 100);
            foreach (var b in bookings)
            {
                b.UserInfo = await _userClient.GetUserByIdAsync(b.UserId, token);
                b.VehicleInfo = await _vehicleClient.GetVehicleByIdAsync(b.VehicleId, token);
            }
            return bookings;
        }

        // ------------------- CANCEL BOOKING -------------------
        public async Task<bool> CancelBookingAsync(string id)
        {
            var booking = await _repo.GetByIdAsync(id);
            if (booking == null) return false;

            if (booking.Status == BookingStatus.Cancelled || booking.Status == BookingStatus.Completed)
                throw new Exception("Không thể hủy booking này.");

            booking.Status = BookingStatus.Cancelled;
            booking.UpdatedAt = DateTime.UtcNow;
            await _repo.UpdateAsync(id, booking);

            _logger.LogInformation("🚫 Booking {Id} has been cancelled", id);
            return true;
        }

        // ------------------- CONFIRM BOOKING -------------------
        public async Task<bool> ConfirmBookingAsync(string id)
        {
            var booking = await _repo.GetByIdAsync(id);
            if (booking == null) return false;

            if (booking.Status != BookingStatus.Pending)
                throw new Exception("Booking này không ở trạng thái chờ xác nhận.");

            booking.Status = BookingStatus.Confirmed;
            booking.UpdatedAt = DateTime.UtcNow;
            await _repo.UpdateAsync(id, booking);

            _logger.LogInformation("✅ Booking {Id} has been confirmed", id);
            return true;
        }
    }
}
