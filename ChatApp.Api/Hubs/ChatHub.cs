using ChatApp.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace ChatApp.Api.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly IMessageService _messageService;
        private readonly IUserContext _userContext;

        public ChatHub(IMessageService messageService, IUserContext userContext)
        {
            _messageService = messageService;
            _userContext = userContext;
        }

        public async Task JoinChatGroup(int chatId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"chat_{chatId}");
        }

        public async Task LeaveChatGroup(int chatId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"chat_{chatId}");
        }

        public async Task SendMessageToChat(int chatId, string messageText)
        {
            var user = await _userContext.GetCurrentUserAsync(Context.User!);
            var messageDto = await _messageService.SendMessageAsync(chatId, user.Id, messageText);
            await Clients.Group($"chat_{chatId}").SendAsync("ReceiveMessage", messageDto);
        }

        public async Task DeleteMessageFromChat(int chatId, int messageId)
        {
            var user = await _userContext.GetCurrentUserAsync(Context.User!);
            await _messageService.DeleteMessageAsync(chatId, messageId, user.Id);
            await Clients.Group($"chat_{chatId}").SendAsync("MessageDeleted", messageId);
        }
    }
}
