using ChatApp.Application.DTOs.Messages;

namespace ChatApp.Application.Interfaces
{
    public interface IMessageService
    {
        Task<IEnumerable<MessageDto>> GetChatMessagesAsync(int chatId, int userId);
        Task<IEnumerable<MessageDto>> SearchMessagesAsync(int userId, string keyword);
        Task<IEnumerable<MessageDto>> SearchMessagesByChatAsync(int chatId, int userId, string keyword);
        Task<MessageDto> SendMessageAsync(int chatId, int userId, string text);
        Task DeleteMessageAsync(int chatId, int messageId, int userId);
        Task<MessageDto> EditMessageAsync(int chatId, int messageId, int userId, string newText);
    }
}
