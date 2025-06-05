using ChatApp.Application.DTOs.Chats;
using ChatApp.Application.Extensions;
using ChatApp.Application.Interfaces;
using ChatApp.Domain.Entities;
using ChatApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.Infrastructure.Services
{
    public class ChatService : IChatService
    {
        private readonly ApplicationDbContext _context;

        public ChatService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ChatDto>> GetUserChatsAsync(int userId)
        {
            var chats = await _context.Chats
                .Include(c => c.ChatUsers)
                .Where(c => c.ChatUsers.Any(cu => cu.UserId == userId))
                .ToListAsync();

            return chats.Select(c => c.ToDto()).ToList();
        }

        public async Task<ChatDto?> GetChatByIdAsync(int userId, int chatId)
        {
            var chat = await _context.Chats
                .Include(c => c.ChatUsers)
                .Where(c => c.Id == chatId && c.ChatUsers.Any(cu => cu.UserId == userId))
                .FirstOrDefaultAsync();

            return chat?.ToDto();
        }

        public async Task<ChatDto> CreateChatAsync(int userId, string chatName)
        {
            if (string.IsNullOrWhiteSpace(chatName))
                throw new ArgumentException("Chat name must be provided.", nameof(chatName));

            var chat = new Chat
            {
                Name = chatName,
                ChatUsers = new List<ChatUser>()
            };

            var chatUser = new ChatUser
            {
                UserId = userId,
                Chat = chat
            };

            chat.ChatUsers.Add(chatUser);
            _context.Chats.Add(chat);
            await _context.SaveChangesAsync();

            return chat.ToDto();
        }
    }
}
