using ChatApp.Application.DTOs.Chats;
using ChatApp.Application.DTOs.Messages;

namespace ChatApp.Application.Interfaces
{
    public interface IChatService
    {
        Task<IEnumerable<ChatUserDto>> GetUserChatsAsync(int userId);
        Task<ChatUserDto> CreateChatAsync(int userId, string chatName);
        Task<IEnumerable<MessageDto>> GetChatMessagesAsync(int chatId, int userId);
        Task<MessageDto> SendMessageAsync(int chatId, int userId, string text);
    }
}
