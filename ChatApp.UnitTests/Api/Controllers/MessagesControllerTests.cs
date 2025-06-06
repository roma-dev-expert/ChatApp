using ChatApp.Api.Controllers;
using ChatApp.Application.DTOs.Messages;
using ChatApp.Application.Interfaces;
using ChatApp.Domain.Entities;
using ChatApp.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Security.Claims;
using Xunit;

namespace ChatApp.UnitTests.Api.Controllers
{
    public class MessagesControllerTests
    {
        private readonly Mock<IMessageService> _mockMessageService;
        private readonly Mock<IUserContext> _mockUserContext;
        private readonly ApplicationDbContext _dbContext;
        private readonly MessagesController _controller;

        public MessagesControllerTests()
        {
            _mockMessageService = new Mock<IMessageService>();
            _mockUserContext = new Mock<IUserContext>();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "FakeDbForMessagesController")
                .Options;
            _dbContext = new ApplicationDbContext(options);

            _controller = new MessagesController(_dbContext, _mockMessageService.Object, _mockUserContext.Object);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Name, "testuser")
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };

            _mockUserContext.Setup(u => u.GetCurrentUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(new User { Id = 1, Username = "testuser", PasswordHash = "dummy" });
        }

        [Fact]
        public async Task GetChatMessages_ReturnsOk_WithMessages()
        {
            int chatId = 100;
            var messages = new List<MessageDto>
            {
                new MessageDto { Id = 1, ChatId = chatId, Text = "Hello", SentAt = DateTime.UtcNow },
                new MessageDto { Id = 2, ChatId = chatId, Text = "World", SentAt = DateTime.UtcNow }
            };

            _mockMessageService
                .Setup(s => s.GetChatMessagesAsync(chatId, 1, 1, 10))
                .ReturnsAsync(messages);

            var result = await _controller.GetChatMessages(chatId, pageNumber: 1, pageSize: 10);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedMessages = Assert.IsAssignableFrom<IEnumerable<MessageDto>>(okResult.Value);
            Assert.Equal(2, returnedMessages.Count());
        }

        [Fact]
        public async Task GetMessageById_ReturnsOk_WhenMessageExists()
        {
            int chatId = 101, messageId = 5;
            var messageDto = new MessageDto { Id = messageId, ChatId = chatId, Text = "Test Message", SentAt = DateTime.UtcNow };

            _mockMessageService
                .Setup(s => s.GetMessageByIdAsync(chatId, messageId, 1))
                .ReturnsAsync(messageDto);

            var result = await _controller.GetMessageById(chatId, messageId);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedMessage = Assert.IsType<MessageDto>(okResult.Value);
            Assert.Equal("Test Message", returnedMessage.Text);
        }

        [Fact]
        public async Task GetMessageById_ReturnsNotFound_WhenMessageDoesNotExist()
        {
            int chatId = 101, messageId = 999;
            _mockMessageService
                .Setup(s => s.GetMessageByIdAsync(chatId, messageId, 1))
                .ReturnsAsync((MessageDto)null);

            var result = await _controller.GetMessageById(chatId, messageId);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task SendMessage_ReturnsCreatedAtAction_WithMessageDto()
        {
            int chatId = 102;
            var messageDto = new MessageDto { Id = 10, ChatId = chatId, Text = "New Test Message", SentAt = DateTime.UtcNow };

            _mockMessageService
                .Setup(s => s.SendMessageAsync(chatId, 1, "New Test Message"))
                .ReturnsAsync(messageDto);

            var request = new CreateMessageRequest("New Test Message");

            var result = await _controller.SendMessage(chatId, request);

            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(nameof(_controller.GetMessageById), createdAtActionResult.ActionName);
            var returnedMessage = Assert.IsType<MessageDto>(createdAtActionResult.Value);
            Assert.Equal("New Test Message", returnedMessage.Text);
            Assert.Equal(messageDto.ChatId, returnedMessage.ChatId);
            Assert.Equal(messageDto.Id, returnedMessage.Id);
        }

        [Fact]
        public async Task DeleteMessage_ReturnsOk_WithConfirmationMessage()
        {
            int chatId = 103, messageId = 20;
            _mockMessageService
                .Setup(s => s.DeleteMessageAsync(chatId, messageId, 1))
                .Returns(Task.CompletedTask);

            var result = await _controller.DeleteMessage(chatId, messageId);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var confirmation = Assert.IsType<string>(okResult.Value);
            Assert.Contains($"Message with id {messageId} was successfully deleted.", confirmation);
        }

        [Fact]
        public async Task EditMessage_ReturnsOk_WithUpdatedMessageDto()
        {
            int chatId = 104, messageId = 30;
            var updatedDto = new MessageDto { Id = messageId, ChatId = chatId, Text = "Updated Message", SentAt = DateTime.UtcNow };

            _mockMessageService
                .Setup(s => s.EditMessageAsync(chatId, messageId, 1, "Updated Message"))
                .ReturnsAsync(updatedDto);

            var request = new EditMessageRequest("Updated Message");

            var result = await _controller.EditMessage(chatId, messageId, request);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedDto = Assert.IsType<MessageDto>(okResult.Value);
            Assert.Equal("Updated Message", returnedDto.Text);
        }
    }
}
