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

        public async Task<User> GetByEmail(string email)
        {
            return await _context.Users.Find(u => u.Email == email).FirstOrDefaultAsync();
        }

        public async Task<bool> ValidateUser(string email, string password)
        {
            var user = await GetByEmail(email);
            if (user == null) return false;
            return BCrypt.Net.BCrypt.Verify(password, user.Password);
        }

        public async Task<List<User>> GetAllUsers()
        {
            return await _context.Users.Find(_ => true).ToListAsync();
        }
    }
}
