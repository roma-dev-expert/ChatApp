using ChatApp.Application.Interfaces;
using ChatApp.Domain.Exceptions;
using ChatApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.Infrastructure.Services
{
    public class ChatParticipationService : IChatParticipationService
    {
        private readonly ApplicationDbContext _context;

        public ChatParticipationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task EnsureUserIsParticipantAsync(int chatId, int userId)
        {
            bool isParticipant = await _context.ChatUsers
                .AnyAsync(cu => cu.ChatId == chatId && cu.UserId == userId);
            if (!isParticipant)
            {
                throw new ForbiddenException("You are not a participant of this chat.");
            }
        }
    }
}
