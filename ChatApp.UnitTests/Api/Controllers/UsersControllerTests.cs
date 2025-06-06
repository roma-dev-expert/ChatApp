using ChatApp.Application.DTOs.Users;
using ChatApp.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace ChatApp.UnitTests.Api.Controllers
{
    public class UsersControllerTests
    {
        private readonly Mock<IAuthService> _mockAuthService;
        private readonly UsersController _controller;

        public UsersControllerTests()
        {
            _mockAuthService = new Mock<IAuthService>();
            _controller = new UsersController(_mockAuthService.Object);
        }

        [Fact]
        public async Task Register_ReturnsCreated_WithSuccessMessage()
        {
            var registerRequest = new RegisterRequest("testuser", "Password!123");

            _mockAuthService
                .Setup(s => s.RegisterAsync(registerRequest))
                .Returns(Task.CompletedTask);

            var result = await _controller.Register(registerRequest);

            var createdResult = Assert.IsType<CreatedResult>(result);

            var value = createdResult.Value;
            var type = value.GetType();
            var messageProp = type.GetProperty("Message");
            Assert.NotNull(messageProp);
            var message = messageProp.GetValue(value) as string;

            Assert.Equal("Registration successful.", message);
        }


        [Fact]
        public async Task Login_ReturnsOk_WithLoginResponse()
        {
            var loginRequest = new LoginRequest("testuser", "Password!123");

            var expectedResponse = new LoginResponse("FakeJwtToken123");

            _mockAuthService
                .Setup(s => s.LoginAsync(loginRequest))
                .ReturnsAsync(expectedResponse);

            var result = await _controller.Login(loginRequest);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var actualResponse = Assert.IsType<LoginResponse>(okResult.Value);
            Assert.Equal(expectedResponse.Token, actualResponse.Token);
        }
    }
}
