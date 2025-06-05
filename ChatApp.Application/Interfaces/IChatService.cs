using ChatApp.Application.DTOs.Chats;

namespace ChatApp.Application.Interfaces
{
    public interface IChatService
    {
        Task<IEnumerable<ChatUserDto>> GetUserChatsAsync(int userId);
        Task<ChatUserDto?> GetChatByIdAsync(int userId, int chatId);
        Task<ChatUserDto> CreateChatAsync(int userId, string chatName);
    }
}
