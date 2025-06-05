using ChatApp.Domain.Entities;
using ChatApp.Application.DTOs.Messages;

namespace ChatApp.Application.Extensions
{
    public static class MessageExtensions
    {
        public static MessageDto ToDto(this Message message)
        {
            if (message is null)
                throw new ArgumentNullException(nameof(message));

            return new MessageDto
            {
                Id = message.Id,
                ChatId = message.ChatId,
                UserId = message.UserId,
                Text = message.Text,
                SentAt = message.SentAt
            };
        }
    }
}
