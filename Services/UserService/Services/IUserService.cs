using UserService.Models;

namespace UserService.Services
{
    public interface IUserService
    {
        Task<IEnumerable<User>> GetAllAsync();
        Task<User?> GetByIdAsync(int id);
        Task<User> RegisterAsync(User user);
        Task<User?> LoginAsync(string email, string password);
    }
}
