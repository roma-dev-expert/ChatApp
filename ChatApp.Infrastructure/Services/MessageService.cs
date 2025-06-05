using ChatApp.Application.DTOs.Messages;
using ChatApp.Application.Extensions;
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

        public async Task<IEnumerable<MessageDto>> GetChatMessagesAsync(int chatId, int userId, int pageNumber, int pageSize)
        {
            await _chatParticipationService.EnsureUserIsParticipantAsync(chatId, userId);

            var messages = await _context.Messages
                .Where(m => m.ChatId == chatId)
                .OrderBy(m => m.SentAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return messages.Select(m => m.ToDto()).ToList();
        }
        public async Task<MessageDto?> GetMessageByIdAsync(int chatId, int messageId, int userId)
        {
            await _chatParticipationService.EnsureUserIsParticipantAsync(chatId, userId);

            var message = await _context.Messages
                .Where(m => m.ChatId == chatId && m.Id == messageId)
                .FirstOrDefaultAsync();

            return message?.ToDto();
        }

        public async Task<IEnumerable<MessageDto>> SearchMessagesAsync(int userId, string keyword, int pageNumber, int pageSize)
        {
            var messages = await _context.Messages
                .Where(m => m.Text.Contains(keyword) &&
                            _context.ChatUsers.Any(cu => cu.ChatId == m.ChatId && cu.UserId == userId))
                .OrderBy(m => m.SentAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return messages.Select(m => m.ToDto()).ToList();
        }

        public async Task<IEnumerable<MessageDto>> SearchMessagesByChatAsync(int chatId, int userId, string keyword, int pageNumber, int pageSize)
        {
            await _chatParticipationService.EnsureUserIsParticipantAsync(chatId, userId);

            var messages = await _context.Messages
                .Where(m => m.ChatId == chatId && m.Text.Contains(keyword))
                .OrderBy(m => m.SentAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return messages.Select(m => m.ToDto()).ToList();
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

            return message.ToDto();
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

            return message.ToDto();
        }
    }
}
