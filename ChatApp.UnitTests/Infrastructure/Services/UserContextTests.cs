using System.Security.Claims;
using ChatApp.Domain.Entities;
using ChatApp.Infrastructure.Persistence;
using ChatApp.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ChatApp.UnitTests.Infrastructure.Services
{
    public class UserContextTests
    {
        private ApplicationDbContext CreateInMemoryContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: dbName) // Требуется пакет Microsoft.EntityFrameworkCore.InMemory
                .Options;
            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task GetCurrentUserAsync_ReturnsUser_WhenTokenIsValid()
        {
            string dbName = Guid.NewGuid().ToString();
            using var context = CreateInMemoryContext(dbName);

            var user = new User { Id = 1, Username = "testuser", PasswordHash = "dummyhash" };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "1")
            };
            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);

            var userContext = new UserContext(context);

            var resultUser = await userContext.GetCurrentUserAsync(principal);

            Assert.NotNull(resultUser);
            Assert.Equal(1, resultUser.Id);
            Assert.Equal("testuser", resultUser.Username);
        }

        [Fact]
        public async Task GetCurrentUserAsync_ThrowsUnauthorizedAccessException_WhenUserIdClaimIsMissing()
        {
            string dbName = Guid.NewGuid().ToString();
            using var context = CreateInMemoryContext(dbName);

            var identity = new ClaimsIdentity();
            var principal = new ClaimsPrincipal(identity);

            var userContext = new UserContext(context);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => userContext.GetCurrentUserAsync(principal));
        }

        [Fact]
        public async Task GetCurrentUserAsync_ThrowsArgumentException_WhenClaimValueIsInvalid()
        {
            string dbName = Guid.NewGuid().ToString();
            using var context = CreateInMemoryContext(dbName);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "abc")
            };
            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);

            var userContext = new UserContext(context);

            var ex = await Assert.ThrowsAsync<ArgumentException>(() => userContext.GetCurrentUserAsync(principal));
            Assert.Equal("Invalid user ID format.", ex.Message);
        }

        [Fact]
        public async Task GetCurrentUserAsync_ThrowsKeyNotFoundException_WhenUserNotFoundInDatabase()
        {
            string dbName = Guid.NewGuid().ToString();
            using var context = CreateInMemoryContext(dbName);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "1")
            };
            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);

            var userContext = new UserContext(context);

            var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() => userContext.GetCurrentUserAsync(principal));
            Assert.Equal("User not found in the database.", ex.Message);
        }
    }
}
