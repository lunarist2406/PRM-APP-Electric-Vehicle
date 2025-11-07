using AIService.Data;
using AIService.Models;
using MongoDB.Driver;

namespace AIService.Repositories
{
    public class ChatHistoryRepository
    {
        private readonly MongoDbContext _context;

        public ChatHistoryRepository(MongoDbContext context)
        {
            _context = context;
        }

        public async Task AddChatAsync(ChatHistory chat)
        {
            await _context.ChatHistories.InsertOneAsync(chat);
        }

        public async Task<List<ChatHistory>> GetUserChatsAsync(string userId)
        {
            return await _context.ChatHistories
                .Find(c => c.UserId == userId)
                .SortByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task DeleteUserChatsAsync(string userId)
        {
            await _context.ChatHistories.DeleteManyAsync(c => c.UserId == userId);
        }
    }
}
