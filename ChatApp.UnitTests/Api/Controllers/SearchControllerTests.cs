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
    public class SearchControllerTests
    {
        private readonly Mock<IMessageService> _mockMessageService;
        private readonly Mock<IUserContext> _mockUserContext;
        private readonly ApplicationDbContext _dbContext;
        private readonly SearchController _controller;

        public SearchControllerTests()
        {
            _mockMessageService = new Mock<IMessageService>();
            _mockUserContext = new Mock<IUserContext>();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "FakeDbForSearchController")
                .Options;
            _dbContext = new ApplicationDbContext(options);

            _controller = new SearchController(_dbContext, _mockMessageService.Object, _mockUserContext.Object);

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

            _mockUserContext
                .Setup(u => u.GetCurrentUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(new User { Id = 1, Username = "testuser", PasswordHash = "dummy" });
        }

        [Fact]
        public async Task SearchMessages_ReturnsOk_WithMessageList()
        {
            string keyword = "hello";
            int pageNumber = 1, pageSize = 10;
            var messages = new List<MessageDto>
            {
                new MessageDto { Id = 1, ChatId = 100, Text = "hello world", SentAt = DateTime.UtcNow },
                new MessageDto { Id = 2, ChatId = 101, Text = "say hello", SentAt = DateTime.UtcNow }
            };

            _mockMessageService
                .Setup(s => s.SearchMessagesAsync(1, keyword, pageNumber, pageSize))
                .ReturnsAsync(messages);

            var result = await _controller.SearchMessages(keyword, pageNumber, pageSize);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedMessages = Assert.IsAssignableFrom<IEnumerable<MessageDto>>(okResult.Value);
            Assert.Equal(2, returnedMessages.Count());
        }

        [Fact]
        public async Task SearchMessagesByChat_ReturnsOk_WithMessageList()
        {
            int chatId = 200, pageNumber = 1, pageSize = 10;
            string keyword = "test";
            var messages = new List<MessageDto>
            {
                new MessageDto { Id = 3, ChatId = chatId, Text = "This is a test message", SentAt = DateTime.UtcNow },
                new MessageDto { Id = 4, ChatId = chatId, Text = "Another test", SentAt = DateTime.UtcNow }
            };

            _mockMessageService
                .Setup(s => s.SearchMessagesByChatAsync(chatId, 1, keyword, pageNumber, pageSize))
                .ReturnsAsync(messages);

            var result = await _controller.SearchMessagesByChat(chatId, keyword, pageNumber, pageSize);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedMessages = Assert.IsAssignableFrom<IEnumerable<MessageDto>>(okResult.Value);
            Assert.Equal(2, returnedMessages.Count());
        }
    }
}
