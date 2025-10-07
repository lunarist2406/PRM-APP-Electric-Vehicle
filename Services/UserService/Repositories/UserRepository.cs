using Microsoft.EntityFrameworkCore;
using UserService.Data;
using UserService.Models;

namespace UserService.Repositories
{
	public class UserRepository : IUserRepository
	{
		private readonly AppDbContext _context;

		public UserRepository(AppDbContext context)
		{
			_context = context;
		}

		public async Task<IEnumerable<User>> GetAllAsync()
			=> await _context.Users.ToListAsync();

		public async Task<User?> GetByIdAsync(int id)
			=> await _context.Users.FindAsync(id);

		public async Task<User?> GetByEmailAsync(string email)
			=> await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

		public async Task<User> AddAsync(User user)
		{
			_context.Users.Add(user);
			await _context.SaveChangesAsync();
			return user;
		}

		public async Task<User?> UpdateAsync(User user)
		{
			_context.Users.Update(user);
			await _context.SaveChangesAsync();
			return user;
		}

		public async Task<bool> DeleteAsync(int id)
		{
			var existing = await _context.Users.FindAsync(id);
			if (existing == null) return false;

			_context.Users.Remove(existing);
			await _context.SaveChangesAsync();
			return true;
		}
	}
}
