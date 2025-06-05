using ChatApp.Application.DTOs.Chats;
using ChatApp.Domain.Entities;

namespace ChatApp.Application.Extensions
{
    public static class ChatExtensions
    {
        public static ChatDto ToDto(this Chat chat)
        {
            if (chat is null)
                throw new ArgumentNullException(nameof(chat));

            return new ChatDto
            {
                Id = chat.Id,
                Name = chat.Name
            };
        }
    }
}
