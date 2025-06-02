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
            var user = await GetCurrentUser();

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

            var user = await GetCurrentUser();

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

        public class CreateChatRequest
        {
            public required string Name { get; set; }
        }
    }
}
