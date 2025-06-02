using ChatApp.Application.DTOs;
using ChatApp.Domain.Entities;
using ChatApp.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.Api.Controllers
{
    [Route("api/[controller]")]
    public class ChatsController : BaseController
    {
        public ChatsController(ApplicationDbContext context) : base(context)
        {
        }

        [HttpGet]
        public async Task<IActionResult> GetUserChats()
        {
            var user = await GetCurrentUserAsync();

            var chats = await _context.Chats
                .Include(c => c.ChatUsers)
                .Where(c => c.ChatUsers.Any(cu => cu.UserId == user.Id))
                .Select(c => new ChatUserDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    ParticipantIds = c.ChatUsers.Select(cu => cu.UserId).ToList()
                })
                .ToListAsync();

            return Ok(chats);
        }

        [HttpPost]
        public async Task<IActionResult> CreateChat([FromBody] CreateChatRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return BadRequest("Chat name must be provided.");
            }

            var user = await GetCurrentUserAsync();

            var chat = new Chat
            {
                Name = request.Name,
                ChatUsers = new List<ChatUser>()
            };

            var chatUser = new ChatUser
            {
                UserId = user.Id,
                User = user,
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

            return CreatedAtAction(nameof(GetUserChats), new { id = chat.Id }, result);
        }

        [HttpGet("{chatId}/messages")]
        public async Task<IActionResult> GetChatMessages(int chatId)
        {
            var user = await GetCurrentUserAsync();

            bool isParticipant = await _context.ChatUsers
                .AnyAsync(cu => cu.ChatId == chatId && cu.UserId == user.Id);

            if (!isParticipant)
            {
                return StatusCode(StatusCodes.Status403Forbidden, "You are not a participant of this chat.");
            }

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

            return Ok(messages);
        }

        public class CreateChatRequest
        {
            public required string Name { get; set; }
        }
    }
}
