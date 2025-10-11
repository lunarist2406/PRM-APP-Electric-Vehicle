using MongoDB.Driver;
using BCrypt.Net;
using UserService.Data;
using UserService.Models;

namespace UserService.Services
{
    public class UserService
    {
        private readonly MongoDbContext _context;

        public UserService(MongoDbContext context)
        {
            _context = context;
        }

        public async Task<User> CreateUser(string name, string email, string password, string role)
        {
            var hashed = BCrypt.Net.BCrypt.HashPassword(password);
            var user = new User { Name = name, Email = email, Password = hashed, Role = role };
            await _context.Users.InsertOneAsync(user);
            return user;
        }

        public async Task<User?> GetByEmail(string email)
        {
            return await _context.Users.Find(u => u.Email == email).FirstOrDefaultAsync();
        }

        public async Task<User?> GetById(string id)
        {
            return await _context.Users.Find(u => u.Id == id).FirstOrDefaultAsync();
        }

        public async Task<List<User>> GetAllUsers()
        {
            return await _context.Users.Find(_ => true).ToListAsync();
        }

        public async Task<bool> ValidateUser(string email, string password)
        {
            var user = await GetByEmail(email);
            if (user == null) return false;
            return BCrypt.Net.BCrypt.Verify(password, user.Password);
        }

        public async Task<bool> UpdateUser(string id,string name, string email, string role)
        {
            var update = Builders<User>.Update
                .Set(u => u.Name, name)
                .Set(u => u.Email, email)
                .Set(u => u.Role, role);

            var result = await _context.Users.UpdateOneAsync(u => u.Id == id, update);
            return result.ModifiedCount > 0;
        }
        public async Task<bool> UpdateProfile(string id, string name, string email, string password)
        {
            var updateDef = Builders<User>.Update
                .Set(u => u.Name, name)
                .Set(u => u.Email, email);

            // Nếu có password mới thì mã hóa rồi cập nhật
            if (!string.IsNullOrWhiteSpace(password))
            {
                var hashed = BCrypt.Net.BCrypt.HashPassword(password);
                updateDef = updateDef.Set(u => u.Password, hashed);
            }

            var result = await _context.Users.UpdateOneAsync(u => u.Id == id, updateDef);
            return result.ModifiedCount > 0;
        }


        public async Task<bool> DeleteUser(string id)
        {
            var result = await _context.Users.DeleteOneAsync(u => u.Id == id);
            return result.DeletedCount > 0;
        }
    }
}
