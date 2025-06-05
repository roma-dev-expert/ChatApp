using ChatApp.Application.DTOs.Chats;
using ChatApp.Application.Interfaces;
using ChatApp.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;

namespace ChatApp.Api.Controllers
{
    [Route("api/chats")]
    public class ChatsController : BaseController
    {
        private readonly IChatService _chatService;

        public ChatsController(ApplicationDbContext context, IChatService chatService, IUserContext userContext)
            : base(context, userContext)
        {
            _chatService = chatService;
        }

        [HttpGet]
        public async Task<IActionResult> GetUserChats()
        {
            var user = await GetCurrentUserAsync();
            var chats = await _chatService.GetUserChatsAsync(user.Id);
            return Ok(chats);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetChatById(int id)
        {
            var user = await GetCurrentUserAsync();
            var chat = await _chatService.GetChatByIdAsync(user.Id, id);
            if (chat == null)
                return NotFound();
            return Ok(chat);
        }

        [HttpPost]
        public async Task<IActionResult> CreateChat([FromBody] CreateChatRequest request)
        {
            var user = await GetCurrentUserAsync();
            var chatDto = await _chatService.CreateChatAsync(user.Id, request.Name);
            return CreatedAtAction(nameof(GetChatById), new { id = chatDto.Id }, chatDto);
        }
    }
}
