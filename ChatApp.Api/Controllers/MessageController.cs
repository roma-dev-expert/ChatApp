using ChatApp.Application.DTOs.Messages;
using ChatApp.Application.Interfaces;
using ChatApp.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChatApp.Api.Controllers
{
    [Route("api/chats/{chatId}/messages")]
    [Authorize]
    public class MessagesController : BaseController
    {
        private readonly IMessageService _messageService;

        public MessagesController(ApplicationDbContext context, IMessageService messageService, IUserContext userContext)
            : base(context, userContext)
        {
            _messageService = messageService;
        }

        [HttpGet]
        public async Task<IActionResult> GetChatMessages(int chatId, int pageNumber, int pageSize)
        {
            var user = await GetCurrentUserAsync();
            var messages = await _messageService.GetChatMessagesAsync(chatId, user.Id, pageNumber, pageSize);
            return Ok(messages);
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage(int chatId, [FromBody] CreateMessageRequest request)
        {
            var user = await GetCurrentUserAsync();
            var messageDto = await _messageService.SendMessageAsync(chatId, user.Id, request.Text);
            return CreatedAtAction(nameof(GetChatMessages), new { chatId = messageDto.ChatId }, messageDto);
        }

        [HttpDelete("{messageId}")]
        public async Task<IActionResult> DeleteMessage(int chatId, int messageId)
        {
            var user = await GetCurrentUserAsync();
            await _messageService.DeleteMessageAsync(chatId, messageId, user.Id);
            return Ok($"Message with id {messageId} was successfully deleted.");
        }

        [HttpPut("{messageId}")]
        public async Task<IActionResult> EditMessage(int chatId, int messageId, [FromBody] EditMessageRequest request)
        {
            var user = await GetCurrentUserAsync();
            var updatedMessage = await _messageService.EditMessageAsync(chatId, messageId, user.Id, request.Text);
            return Ok(updatedMessage);
        }
    }
}
