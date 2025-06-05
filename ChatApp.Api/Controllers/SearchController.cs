using ChatApp.Application.Interfaces;
using ChatApp.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;

namespace ChatApp.Api.Controllers
{
    [Route("api/search")]
    public class SearchController : BaseController
    {
        private readonly IMessageService _messageService;

        public SearchController(ApplicationDbContext context, IMessageService messageService, IUserContext userContext)
            : base(context, userContext)
        {
            _messageService = messageService;
        }

        [HttpGet("messages")]
        public async Task<IActionResult> SearchMessages(
            [FromQuery] string keyword,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var user = await GetCurrentUserAsync();
            var messages = await _messageService.SearchMessagesAsync(user.Id, keyword, pageNumber, pageSize);
            return Ok(messages);
        }

        [HttpGet("chats/{chatId}/messages")]
        public async Task<IActionResult> SearchMessagesByChat(
            int chatId,
            [FromQuery] string keyword,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var user = await GetCurrentUserAsync();
            var messages = await _messageService.SearchMessagesByChatAsync(chatId, user.Id, keyword, pageNumber, pageSize);
            return Ok(messages);
        }
    }
}
