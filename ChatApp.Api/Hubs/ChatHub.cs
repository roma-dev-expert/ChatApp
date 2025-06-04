using ChatApp.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace ChatApp.Api.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly IChatService _chatService;
        private readonly IUserContext _userContext;
        private readonly ILogger<ChatHub> _logger;

        public ChatHub(IChatService chatService, IUserContext userContext, ILogger<ChatHub> logger)
        {
            _chatService = chatService;
            _userContext = userContext;
            _logger = logger;
        }

        public async Task JoinChatGroup(int chatId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, chatId.ToString());
        }

        public async Task LeaveChatGroup(int chatId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, chatId.ToString());
        }

        public async Task SendMessageToChat(int chatId, string messageText)
        {
            try
            {
                var user = await _userContext.GetCurrentUserAsync(Context.User!);
                var messageDto = await _chatService.SendMessageAsync(chatId, user.Id, messageText);
                await Clients.Group(chatId.ToString()).SendAsync("ReceiveMessage", messageDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while sending message to chat {ChatId}", chatId);
                await Clients.Caller.SendAsync("ReceiveError", "An error occurred while sending your message.");
            }
        }
    }
}
