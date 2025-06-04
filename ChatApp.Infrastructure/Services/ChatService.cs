using ChatApp.Application.DTOs.Chats;
using ChatApp.Application.DTOs.Messages;
using ChatApp.Application.Interfaces;
using ChatApp.Domain.Entities;
using ChatApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.Infrastructure.Services
{
    public class ChatService : IChatService
    {
        private readonly ApplicationDbContext _context;
        private readonly IChatParticipationService _chatParticipationService;

        public ChatService(ApplicationDbContext context, IChatParticipationService chatParticipationService)
        {
            _context = context;
            _chatParticipationService = chatParticipationService;
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

        public async Task<IEnumerable<MessageDto>> GetChatMessagesAsync(int chatId, int userId)
        {
            await _chatParticipationService.EnsureUserIsParticipantAsync(chatId, userId);

            var messages = await _context.Messages
                .Where(m => m.ChatId == chatId)
                .OrderBy(m => m.SentAt)
                .Select(m => new MessageDto
                {
                    Id = m.Id,
                    ChatId = m.ChatId,
                    UserId = m.UserId,
                    Text = m.Text,
                    SentAt = m.SentAt
                })
                .ToListAsync();

            return messages;
        }

        public async Task<MessageDto> SendMessageAsync(int chatId, int userId, string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                throw new ArgumentException("Message text must be provided.", nameof(text));

            await _chatParticipationService.EnsureUserIsParticipantAsync(chatId, userId);

            bool chatExists = await _context.Chats.AnyAsync(c => c.Id == chatId);
            if (!chatExists)
                throw new KeyNotFoundException("Chat not found.");

            var message = new Message
            {
                ChatId = chatId,
                UserId = userId,
                Text = text,
                SentAt = DateTime.UtcNow
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            var result = new MessageDto
            {
                Id = message.Id,
                ChatId = message.ChatId,
                UserId = message.UserId,
                Text = message.Text,
                SentAt = message.SentAt
            };

            return result;
        }
    }
}
