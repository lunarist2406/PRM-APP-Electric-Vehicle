using BookingService.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BookingService.Repositories
{
    public interface IBookingRepository
    {
        Task<List<Booking>> GetAllAsync(
            string? userId = null,
            string? stationId = null,
            string? vehicleId = null,
            string? chargingPointId = null,
            string? status = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            int page = 1,
            int limit = 10
        );

        Task<Booking?> GetByIdAsync(string id);
        Task CreateAsync(Booking booking);
        Task UpdateAsync(string id, Booking booking);
        Task DeleteAsync(string id);
    }
}
