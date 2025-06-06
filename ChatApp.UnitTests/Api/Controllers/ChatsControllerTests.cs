using ChatApp.Api.Controllers;
using ChatApp.Application.DTOs.Chats;
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
    public class ChatsControllerTests
    {
        private readonly Mock<IChatService> _mockChatService;
        private readonly Mock<IUserContext> _mockUserContext;
        private readonly ApplicationDbContext _dbContext;
        private readonly ChatsController _controller;

        public ChatsControllerTests()
        {
            _mockChatService = new Mock<IChatService>();
            _mockUserContext = new Mock<IUserContext>();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "FakeDbForControllerTests")
                .Options;
            _dbContext = new ApplicationDbContext(options);

            _controller = new ChatsController(_dbContext, _mockChatService.Object, _mockUserContext.Object);

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
        public async Task GetUserChats_ReturnsOk_WithChats()
        {
            var chats = new List<ChatDto>
            {
                new ChatDto { Id = 1, Name = "Chat1" },
                new ChatDto { Id = 2, Name = "Chat2" }
            };
            _mockChatService.Setup(s => s.GetUserChatsAsync(It.IsAny<int>()))
                .ReturnsAsync(chats);

            var result = await _controller.GetUserChats();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedChats = Assert.IsAssignableFrom<IEnumerable<ChatDto>>(okResult.Value);
            Assert.Equal(2, returnedChats.Count());
        }

        [Fact]
        public async Task GetChatById_ReturnsOk_WhenChatExists()
        {
            var chatDto = new ChatDto { Id = 1, Name = "Test Chat" };
            _mockChatService.Setup(s => s.GetChatByIdAsync(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(chatDto);

            var result = await _controller.GetChatById(1);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedChat = Assert.IsType<ChatDto>(okResult.Value);
            Assert.Equal("Test Chat", returnedChat.Name);
        }

        [Fact]
        public async Task GetChatById_ReturnsNotFound_WhenChatDoesNotExist()
        {
            _mockChatService.Setup(s => s.GetChatByIdAsync(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync((ChatDto)null);

            var result = await _controller.GetChatById(999);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task CreateChat_ReturnsCreatedAtAction_WithChatDto()
        {
            var chatDto = new ChatDto { Id = 10, Name = "New Chat" };
            _mockChatService.Setup(s => s.CreateChatAsync(It.IsAny<int>(), It.IsAny<string>()))
                .ReturnsAsync(chatDto);

            var request = new CreateChatRequest("New Chat");

            var result = await _controller.CreateChat(request);

            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(nameof(_controller.GetChatById), createdAtActionResult.ActionName);
            var returnedChat = Assert.IsType<ChatDto>(createdAtActionResult.Value);
            Assert.Equal("New Chat", returnedChat.Name);
            Assert.Equal(10, returnedChat.Id);
        }
    }
}