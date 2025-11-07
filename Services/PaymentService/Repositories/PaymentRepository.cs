using MongoDB.Driver;
using PaymentService.Data;
using PaymentService.Models;

namespace PaymentService.Repositories
{
    public interface IPaymentRepository
    {
        Task<Payment> CreateAsync(Payment payment);
        Task<List<Payment>> GetByUserIdAsync(string userId);
        Task<List<Payment>> GetAllAsync();
        Task<Payment?> GetByIdAsync(string id);
        Task UpdateStatusAsync(string id, string status);
        Task<List<Payment>> GetPaidPaymentsAsync();
        Task<List<Payment>> GetPaidPaymentsByDateRangeAsync(DateTime startDate, DateTime endDate);
    }

    public class PaymentRepository : IPaymentRepository
    {
        private readonly IMongoCollection<Payment> _payments;

        public PaymentRepository(MongoDbContext context)
        {
            _payments = context.Payments;
        }

        public async Task<Payment> CreateAsync(Payment payment)
        {
            await _payments.InsertOneAsync(payment);
            return payment;
        }

        public async Task<List<Payment>> GetByUserIdAsync(string userId)
        {
            return await _payments.Find(p => p.UserId == userId).ToListAsync();
        }

        public async Task<List<Payment>> GetAllAsync()
        {
            return await _payments.Find(_ => true).ToListAsync();
        }

        public async Task<Payment?> GetByIdAsync(string id)
        {
            return await _payments.Find(p => p.Id == id).FirstOrDefaultAsync();
        }

        public async Task UpdateStatusAsync(string id, string status)
        {
            var update = Builders<Payment>.Update.Set(p => p.Status, status);
            await _payments.UpdateOneAsync(p => p.Id == id, update);
        }

        public async Task<List<Payment>> GetPaidPaymentsAsync()
        {
            return await _payments.Find(p => p.Status == "Paid").ToListAsync();
        }

        public async Task<List<Payment>> GetPaidPaymentsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var filter = Builders<Payment>.Filter.And(
                Builders<Payment>.Filter.Eq(p => p.Status, "Paid"),
                Builders<Payment>.Filter.Gte(p => p.CreatedAt, startDate),
                Builders<Payment>.Filter.Lte(p => p.CreatedAt, endDate)
            );
            return await _payments.Find(filter).ToListAsync();
        }
    }
}
