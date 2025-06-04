using ChatApp.Application.DTOs.Messages;
using ChatApp.Application.Interfaces;
using ChatApp.Domain.Entities;
using ChatApp.Domain.Exceptions;
using ChatApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.Infrastructure.Services
{
    public class MessageService : IMessageService
    {
        private readonly ApplicationDbContext _context;
        private readonly IChatParticipationService _chatParticipationService;

        public MessageService(ApplicationDbContext context, IChatParticipationService chatParticipationService)
        {
            _context = context;
            _chatParticipationService = chatParticipationService;
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

        public async Task DeleteMessageAsync(int chatId, int messageId, int userId)
        {
            await _chatParticipationService.EnsureUserIsParticipantAsync(chatId, userId);

            var message = await _context.Messages.FirstOrDefaultAsync(m => m.Id == messageId && m.ChatId == chatId);
            if (message == null)
                throw new KeyNotFoundException("Message not found.");

            if (message.UserId != userId)
                throw new ForbiddenException("You are not authorized to delete this message.");


            _context.Messages.Remove(message);
            await _context.SaveChangesAsync();
        }

        public async Task<MessageDto> EditMessageAsync(int chatId, int messageId, int userId, string newText)
        {
            if (string.IsNullOrWhiteSpace(newText))
                throw new ArgumentException("New message text must be provided.", nameof(newText));

            await _chatParticipationService.EnsureUserIsParticipantAsync(chatId, userId);

            var message = await _context.Messages.FirstOrDefaultAsync(m => m.Id == messageId && m.ChatId == chatId);
            if (message == null)
                throw new KeyNotFoundException("Message not found.");

            if (message.UserId != userId)
                throw new ForbiddenException("You are not authorized to edit this message.");
            message.Text = newText;

            await _context.SaveChangesAsync();

            return new MessageDto
            {
                Id = message.Id,
                ChatId = message.ChatId,
                UserId = message.UserId,
                Text = message.Text,
                SentAt = message.SentAt
            };
        }
    }
}
