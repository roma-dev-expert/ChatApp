using ChatApp.Application.DTOs.Chats;

namespace ChatApp.Application.Interfaces
{
    public interface IChatService
    {
        Task<IEnumerable<ChatDto>> GetUserChatsAsync(int userId);
        Task<ChatDto?> GetChatByIdAsync(int userId, int chatId);
        Task<ChatDto> CreateChatAsync(int userId, string chatName);
    }
}
