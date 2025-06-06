using Xunit;
using ChatApp.Application.Extensions;
using ChatApp.Application.DTOs.Messages;
using ChatApp.Domain.Entities;

namespace ChatApp.UnitTests.Application.Extensions
{
    public class MessageExtensionsTests
    {
        [Fact]
        public void ToDto_NullMessage_ThrowsArgumentNullException()
        {
            Message nullMessage = null;

            Assert.Throws<ArgumentNullException>(() => nullMessage.ToDto());
        }

        [Fact]
        public void ToDto_ValidMessage_ReturnsMessageDtoWithSameProperties()
        {
            var message = new Message
            {
                Id = 100,
                ChatId = 1,
                UserId = 42,
                Text = "Test message",
                SentAt = DateTime.UtcNow
            };

            MessageDto dto = message.ToDto();

            Assert.NotNull(dto);
            Assert.Equal(message.Id, dto.Id);
            Assert.Equal(message.ChatId, dto.ChatId);
            Assert.Equal(message.UserId, dto.UserId);
            Assert.Equal(message.Text, dto.Text);
            Assert.Equal(message.SentAt, dto.SentAt);
        }
    }
}
