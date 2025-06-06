using System.IdentityModel.Tokens.Jwt;
using ChatApp.Application.DTOs.Users;
using ChatApp.Domain.Entities;
using ChatApp.Infrastructure.Persistence;
using ChatApp.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace ChatApp.UnitTests.Infrastructure.Services
{
    public class AuthServiceTests
    {
        private ApplicationDbContext CreateInMemoryContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: dbName) 
                .Options;
            return new ApplicationDbContext(options);
        }

        private IConfiguration BuildTestConfiguration()
        {
            var configData = new Dictionary<string, string?>
            {
                { "Jwt:ExpirationHours", "1" },
                { "Jwt:Issuer", "TestIssuer" },
                { "Jwt:Audience", "TestAudience" },
                { "Jwt:Key", "a89321b4fbb1bd438b7bb9f9d8f9d86f194cac783e0a9b1f1ce17197b4d46b3b97e0703f31b42c716ec614a1fafe6715c20996d98a10bf0cb8788daac96126eda7a656a8a62ac79cda69315eb26255b624fedb398fd417153d3b7aa73159141d2c4fde6113515fba25240b59ea43d93c1071488cbace1f69ce2f1c74d23919bb" }
            };
            return new ConfigurationBuilder()
                .AddInMemoryCollection(configData)
                .Build();
        }

        [Fact]
        public async Task RegisterAsync_Success()
        {
            string dbName = Guid.NewGuid().ToString();
            using var context = CreateInMemoryContext(dbName);
            var configuration = BuildTestConfiguration();
            var jwtService = new JwtService(configuration, NullLogger<JwtService>.Instance);
            ILogger<AuthService> logger = NullLogger<AuthService>.Instance;
            var authService = new AuthService(context, configuration, jwtService, logger);

            var registerRequest = new RegisterRequest("newuser", "Password123!");

            await authService.RegisterAsync(registerRequest);

            var user = await context.Users.FirstOrDefaultAsync(u => u.Username == "newuser");
            Assert.NotNull(user);
            Assert.True(BCrypt.Net.BCrypt.Verify("Password123!", user.PasswordHash));
        }

        [Fact]
        public async Task RegisterAsync_ExistingUser_ThrowsArgumentException()
        {
            string dbName = Guid.NewGuid().ToString();
            using var context = CreateInMemoryContext(dbName);
            var configuration = BuildTestConfiguration();
            var jwtService = new JwtService(configuration, NullLogger<JwtService>.Instance);
            ILogger<AuthService> logger = NullLogger<AuthService>.Instance;

            var existingUser = new User
            {
                Username = "existinguser",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!")
            };
            context.Users.Add(existingUser);
            await context.SaveChangesAsync();

            var authService = new AuthService(context, configuration, jwtService, logger);
            var registerRequest = new RegisterRequest("existinguser", "AnotherPassword!");

            var exception = await Assert.ThrowsAsync<ArgumentException>(() => authService.RegisterAsync(registerRequest));
            Assert.Equal("User already exists.", exception.Message);
        }

        [Fact]
        public async Task LoginAsync_Success()
        {
            string dbName = Guid.NewGuid().ToString();
            using var context = CreateInMemoryContext(dbName);
            var configuration = BuildTestConfiguration();
            var jwtService = new JwtService(configuration, NullLogger<JwtService>.Instance);
            ILogger<AuthService> logger = NullLogger<AuthService>.Instance;

            var password = "Password123!";
            var user = new User
            {
                Username = "testuser",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password)
            };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var authService = new AuthService(context, configuration, jwtService, logger);
            var loginRequest = new LoginRequest("testuser", password);

            var loginResponse = await authService.LoginAsync(loginRequest);

            Assert.NotNull(loginResponse);
            Assert.False(string.IsNullOrEmpty(loginResponse.Token));

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.ReadJwtToken(loginResponse.Token);

            Assert.Contains(token.Claims, c => c.Type == "unique_name" && c.Value == "testuser");
            Assert.Contains(token.Claims, c => c.Type == "nameid" && c.Value == user.Id.ToString());
        }


        [Fact]
        public async Task LoginAsync_InvalidCredentials_ThrowsUnauthorizedAccessException()
        {
            string dbName = Guid.NewGuid().ToString();
            using var context = CreateInMemoryContext(dbName);
            var configuration = BuildTestConfiguration();
            var jwtService = new JwtService(configuration, NullLogger<JwtService>.Instance);
            ILogger<AuthService> logger = NullLogger<AuthService>.Instance;

            var password = "Password123!";
            var user = new User
            {
                Username = "testuser",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password)
            };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var authService = new AuthService(context, configuration, jwtService, logger);
            var loginRequest = new LoginRequest("testuser", "WrongPassword");

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => authService.LoginAsync(loginRequest));
        }
    }
}
