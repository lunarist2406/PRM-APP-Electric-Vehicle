using UserService.Models;
using UserService.Repositories;

namespace UserService.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _repo;

        public UserService(IUserRepository repo)
        {
            _repo = repo;
        }

        public async Task<IEnumerable<User>> GetAllAsync()
            => await _repo.GetAllAsync();

        public async Task<User?> GetByIdAsync(int id)
            => await _repo.GetByIdAsync(id);

        public async Task<User> RegisterAsync(User user)
        {
            // Simple hash giả lập
            user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
            return await _repo.AddAsync(user);
        }

        public async Task<User?> LoginAsync(string email, string password)
        {
            var existing = await _repo.GetByEmailAsync(email);
            if (existing == null) return null;

            return BCrypt.Net.BCrypt.Verify(password, existing.Password) ? existing : null;
        }
    }
}
