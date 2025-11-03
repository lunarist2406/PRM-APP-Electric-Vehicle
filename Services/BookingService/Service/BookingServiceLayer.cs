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
        private readonly IHttpContextAccessor _httpContextAccessor;

        public BookingServiceLayer(
            IBookingRepository repo,
            UserClient userClient,
            VehicleClient vehicleClient,
            StationClient stationClient,
            ChargingPointClient chargingPointClient,
            ILogger<BookingServiceLayer> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _repo = repo;
            _userClient = userClient;
            _vehicleClient = vehicleClient;
            _stationClient = stationClient;
            _chargingPointClient = chargingPointClient;
            _logger = logger;
             _httpContextAccessor = httpContextAccessor;
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

            var userCache = new Dictionary<string, UserResponseDto>();
            var vehicleCache = new Dictionary<string, VehicleResponseDto>();
            var stationCache = new Dictionary<string, StationResponseDto>();
            var pointCache = new Dictionary<string, ChargingPointResponseDto>();

            foreach (var b in bookings)
            {
                if (!userCache.ContainsKey(b.UserId))
                    userCache[b.UserId] = await _userClient.GetUserByIdAsync(b.UserId, token) ?? new UserResponseDto();

                if (!vehicleCache.ContainsKey(b.VehicleId))
                    vehicleCache[b.VehicleId] = await _vehicleClient.GetVehicleByIdAsync(b.VehicleId, token) ?? new VehicleResponseDto();

                if (!stationCache.ContainsKey(b.StationId))
                    stationCache[b.StationId] = await _stationClient.GetStationByIdAsync(b.StationId, token) ?? new StationResponseDto();

                if (!pointCache.ContainsKey(b.ChargingPointId))
                    pointCache[b.ChargingPointId] = await _chargingPointClient.GetChargingPointByIdAsync(b.ChargingPointId, token) ?? new ChargingPointResponseDto();

                b.UserInfo = userCache[b.UserId]!;
                b.VehicleInfo = vehicleCache[b.VehicleId]!;
                b.StationInfo = stationCache[b.StationId]!;
                b.ChargingPointInfo = pointCache[b.ChargingPointId]!;
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

            // Validate thời gian
            if (dto.StartTime >= dto.EndTime)
                throw new ArgumentException("Thời gian bắt đầu không được trùng với thời gian kết thúc.");

            // Kiểm tra các entity tồn tại
            var user = await _userClient.GetUserByIdAsync(dto.UserId, token)
                ?? throw new Exception($"User {dto.UserId} không tồn tại.");
            var station = await _stationClient.GetStationByIdAsync(dto.StationId, token)
                ?? throw new Exception($"Station {dto.StationId} không tồn tại.");
            var vehicle = await _vehicleClient.GetVehicleByIdAsync(dto.VehicleId, token)
                ?? throw new Exception($"Vehicle {dto.VehicleId} không tồn tại.");
            var charging = await _chargingPointClient.GetChargingPointByIdAsync(dto.ChargingPointId, token)
                ?? throw new Exception($"Charging point {dto.ChargingPointId} không tồn tại.");

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

            // Gán thông tin chi tiết trước khi return
            booking.UserInfo = user;
            booking.StationInfo = station;
            booking.VehicleInfo = vehicle;
            booking.ChargingPointInfo = charging;

            _logger.LogInformation("✅ Booking {Id} created successfully for user {User}", booking.Id, dto.UserId);

            return booking;
        }




        // ------------------- UPDATE -------------------
        public async Task<bool> UpdateAsync(string id, BookingUpdateDto dto)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null)
                return false;

            // Cập nhật các trường hợp có thay đổi
            if (dto.StartTime.HasValue)
                existing.StartTime = dto.StartTime.Value;

            if (dto.EndTime.HasValue)
                existing.EndTime = dto.EndTime.Value;

            // ⚠️ Kiểm tra logic thời gian
            if (existing.StartTime >= existing.EndTime)
                throw new ArgumentException("Thời gian bắt đầu phải nhỏ hơn thời gian kết thúc.");

            if (!string.IsNullOrEmpty(dto.Status))
            {
                if (Enum.TryParse(dto.Status, true, out BookingStatus parsedStatus))
                    existing.Status = parsedStatus;
                else
                    throw new Exception($"Trạng thái không hợp lệ: {dto.Status}");
            }

            if (dto.RateType.HasValue)
                existing.RateType = dto.RateType.Value;

            existing.CalculateTotalFee();
            existing.UpdatedAt = DateTime.UtcNow;

            await _repo.UpdateAsync(id, existing);
            _logger.LogInformation("✅ Booking {Id} updated successfully", id);
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
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentException("UserId cannot be null or empty.", nameof(userId));

            if (string.IsNullOrEmpty(token))
                throw new ArgumentException("Token cannot be null or empty.", nameof(token));

            var bookings = await _repo.GetAllAsync(userId, null, null, null, null, null, null, 1, 100);

            // Cache để tránh gọi API trùng lặp
            var userCache = new Dictionary<string, UserResponseDto>();
            var vehicleCache = new Dictionary<string, VehicleResponseDto>();
            var stationCache = new Dictionary<string, StationResponseDto>();
            var pointCache = new Dictionary<string, ChargingPointResponseDto>();

            foreach (var b in bookings)
            {
                if (!userCache.ContainsKey(b.UserId))
                    userCache[b.UserId] = await _userClient.GetUserByIdAsync(b.UserId, token) ?? new UserResponseDto();

                if (!vehicleCache.ContainsKey(b.VehicleId))
                    vehicleCache[b.VehicleId] = await _vehicleClient.GetVehicleByIdAsync(b.VehicleId, token) ?? new VehicleResponseDto();

                if (!stationCache.ContainsKey(b.StationId))
                    stationCache[b.StationId] = await _stationClient.GetStationByIdAsync(b.StationId, token) ?? new StationResponseDto();

                if (!pointCache.ContainsKey(b.ChargingPointId))
                    pointCache[b.ChargingPointId] = await _chargingPointClient.GetChargingPointByIdAsync(b.ChargingPointId, token) ?? new ChargingPointResponseDto();

                b.UserInfo = userCache[b.UserId]!;
                b.VehicleInfo = vehicleCache[b.VehicleId]!;
                b.StationInfo = stationCache[b.StationId]!;
                b.ChargingPointInfo = pointCache[b.ChargingPointId]!;
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

            _logger.LogInformation("Booking {Id} has been cancelled", id);
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
