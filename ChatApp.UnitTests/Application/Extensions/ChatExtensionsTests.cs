using Xunit;
using ChatApp.Application.Extensions;
using ChatApp.Application.DTOs.Chats;
using ChatApp.Domain.Entities;

namespace ChatApp.UnitTests.Application.Extensions
{
    public class ChatExtensionsTests
    {
        [Fact]
        public void ToDto_NullChat_ThrowsArgumentNullException()
        {
            Chat nullChat = null;

            Assert.Throws<ArgumentNullException>(() => nullChat.ToDto());
        }

        [Fact]
        public void ToDto_ValidChat_ReturnsChatDtoWithSameProperties()
        {
            var chat = new Chat
            {
                Id = 1,
                Name = "Test Chat"
            };

            ChatDto dto = chat.ToDto();

            Assert.NotNull(dto);
            Assert.Equal(chat.Id, dto.Id);
            Assert.Equal(chat.Name, dto.Name);
        }
    }
}
