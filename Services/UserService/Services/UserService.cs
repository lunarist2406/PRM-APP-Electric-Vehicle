using MongoDB.Driver;
using BCrypt.Net;
using UserService.Data;
using UserService.Models;
using UserService.Models.Enums;
using System.Security.Cryptography;

namespace UserService.Services
{
    public class UserService
    {
        private readonly MongoDbContext _context;

        public UserService(MongoDbContext context)
        {
            _context = context;
        }

        // 🔹 Tạo user mới
        public async Task<User> CreateUser(string name, string email, string phone, string password)
        {
            var hashed = BCrypt.Net.BCrypt.HashPassword(password);
            var user = new User
            {
                Name = name,
                Email = email,
                Phone = phone,
                Password = hashed,
                Role = UserRole.Driver,
                IsActive = true
            };
            await _context.Users.InsertOneAsync(user);
            return user;
        }

        // 🔹 Lấy user
        public async Task<User?> GetByEmail(string email)
            => await _context.Users.Find(u => u.Email == email).FirstOrDefaultAsync();

        public async Task<User?> GetById(string id)
            => await _context.Users.Find(u => u.Id == id).FirstOrDefaultAsync();

        public async Task<List<User>> GetAllUsers()
            => await _context.Users.Find(_ => true).ToListAsync();

        // 🔹 Validate login
        public async Task<bool> ValidateUser(string email, string password)
        {
            var user = await GetByEmail(email);
            if (user == null || !user.IsActive) return false;
            return BCrypt.Net.BCrypt.Verify(password, user.Password);
        }

        // 🔹 Update user info (Admin)
        public async Task<bool> UpdateUser(string id, string name, string email, string phone, string role)
        {
            var update = Builders<User>.Update
                .Set(u => u.Name, name)
                .Set(u => u.Email, email)
                .Set(u => u.Phone, phone)
                .Set(u => u.Role, Enum.Parse<UserRole>(role, true));

            var result = await _context.Users.UpdateOneAsync(u => u.Id == id, update);
            return result.ModifiedCount > 0;
        }

        // 🔹 Update profile (user tự đổi)
        public async Task<bool> UpdateProfile(string id, string name, string email, string phone, string password)
        {
            var update = Builders<User>.Update
                .Set(u => u.Name, name)
                .Set(u => u.Email, email)
                .Set(u => u.Phone, phone);

            if (!string.IsNullOrWhiteSpace(password))
                update = update.Set(u => u.Password, BCrypt.Net.BCrypt.HashPassword(password));

            var result = await _context.Users.UpdateOneAsync(u => u.Id == id, update);
            return result.ModifiedCount > 0;
        }

        // 🔹 Delete user
        public async Task<bool> DeleteUser(string id)
        {
            var result = await _context.Users.DeleteOneAsync(u => u.Id == id);
            return result.DeletedCount > 0;
        }

        // 🔹 Generate refresh token
        public string GenerateRefreshToken()
        {
            var randomBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }

        // 🔹 Update refresh token cho user
        public async Task UpdateRefreshToken(string userId, string refreshToken, DateTime expiry)
        {
            var update = Builders<User>.Update
                .Set(u => u.RefreshToken, refreshToken)
                .Set(u => u.RefreshTokenExpiry, expiry);

            await _context.Users.UpdateOneAsync(u => u.Id == userId, update);
        }

        public async Task<User?> GetByRefreshToken(string refreshToken)
            => await _context.Users.Find(u => u.RefreshToken == refreshToken).FirstOrDefaultAsync();

        // 🔹 Logout
        public async Task<bool> Logout(string email)
        {
            var update = Builders<User>.Update
                .Set(u => u.RefreshToken, null)
                .Set(u => u.RefreshTokenExpiry, null);

            var result = await _context.Users.UpdateOneAsync(u => u.Email == email, update);
            return result.ModifiedCount > 0;
        }

        // 🔹 Ban / Unban user
        public async Task<bool> BanUser(string id)
        {
            var result = await _context.Users.UpdateOneAsync(
                u => u.Id == id,
                Builders<User>.Update.Set(u => u.IsActive, false)
            );
            return result.ModifiedCount > 0;
        }

        public async Task<bool> UnbanUser(string id)
        {
            var result = await _context.Users.UpdateOneAsync(
                u => u.Id == id,
                Builders<User>.Update.Set(u => u.IsActive, true)
            );
            return result.ModifiedCount > 0;
        }
        public async Task<bool> UpdateUserRole(string id, UserRole newRole)
        {
            var update = Builders<User>.Update
                .Set(u => u.Role, newRole);

            var result = await _context.Users.UpdateOneAsync(u => u.Id == id, update);
            return result.ModifiedCount > 0;
        }
    }
}
