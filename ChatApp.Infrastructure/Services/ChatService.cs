using ChatApp.Application.DTOs.Chats;
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

        public async Task<IEnumerable<ChatUserDto>> GetUserChatsAsync(int userId)
        {
            var chats = await _context.Chats
                .Include(c => c.ChatUsers)
                .Where(c => c.ChatUsers.Any(cu => cu.UserId == userId))
                .Select(c => new ChatUserDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    ParticipantIds = c.ChatUsers.Select(cu => cu.UserId).ToList()
                })
                .ToListAsync();

            return chats;
        }

        public async Task<ChatUserDto> CreateChatAsync(int userId, string chatName)
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

            var result = new ChatUserDto
            {
                Id = chat.Id,
                Name = chat.Name,
                ParticipantIds = chat.ChatUsers.Select(cu => cu.UserId).ToList()
            };

            return result;
        }
    }
}
