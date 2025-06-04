using ChatApp.Application.DTOs.Chats;
using ChatApp.Application.DTOs.Messages;
using ChatApp.Application.Interfaces;
using ChatApp.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChatApp.Api.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
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

        [HttpPost]
        public async Task<IActionResult> CreateChat([FromBody] CreateChatRequest request)
        {
            var user = await GetCurrentUserAsync();
            var chatDto = await _chatService.CreateChatAsync(user.Id, request.Name);
            return CreatedAtAction(nameof(GetUserChats), new { id = chatDto.Id }, chatDto);
        }

        [HttpGet("{chatId}/messages")]
        public async Task<IActionResult> GetChatMessages(int chatId)
        {
            var user = await GetCurrentUserAsync();
            var messages = await _chatService.GetChatMessagesAsync(chatId, user.Id);
            return Ok(messages);
        }

        [HttpPost("{chatId}/messages")]
        public async Task<IActionResult> SendMessage(int chatId, [FromBody] CreateMessageRequest request)
        {
            var user = await GetCurrentUserAsync();
            var messageDto = await _chatService.SendMessageAsync(chatId, user.Id, request.Text);
            return CreatedAtAction(nameof(GetChatMessages), new { chatId = messageDto.ChatId }, messageDto);
        }
    }
}
