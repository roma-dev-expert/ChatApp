using ChatApp.Application.DTOs;
using ChatApp.Domain.Entities;
using ChatApp.Domain.Exceptions;
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
                ChatId = chat.Id
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

            await EnsureUserIsParticipantAsync(chatId, user.Id);

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

        [HttpPost("{chatId}/messages")]
        public async Task<IActionResult> SendMessage(int chatId, [FromBody] CreateMessageRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Text))
            {
                return BadRequest("Message text must be provided.");
            }

            var user = await GetCurrentUserAsync();

            await EnsureUserIsParticipantAsync(chatId, user.Id);

            var chatExists = await _context.Chats.AnyAsync(c => c.Id == chatId);
            if (!chatExists) return NotFound("Chat not found.");

            var message = new Message
            {
                ChatId = chatId,
                UserId = user.Id,
                Text = request.Text,
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

            return CreatedAtAction(nameof(GetChatMessages), new { chatId = message.ChatId }, result);
        }

        protected async Task EnsureUserIsParticipantAsync(int chatId, int userId)
        {
            bool isParticipant = await _context.ChatUsers
                .AnyAsync(cu => cu.ChatId == chatId && cu.UserId == userId);
            if (!isParticipant)
            {
                throw new ForbiddenException("You are not a participant of this chat.");
            }
        }

        public class CreateMessageRequest
        {
            public required string Text { get; set; }
        }


        public class CreateChatRequest
        {
            public required string Name { get; set; }
        }
    }
}
