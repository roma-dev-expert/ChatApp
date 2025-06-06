using ChatApp.Domain.Entities;
using ChatApp.Domain.Exceptions;
using ChatApp.Infrastructure.Persistence;
using ChatApp.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ChatApp.UnitTests.Infrastructure.Services
{
    public class ChatParticipationServiceTests
    {
        private ApplicationDbContext CreateInMemoryContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: dbName) 
                .Options;
            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task EnsureUserIsParticipantAsync_Succeeds_WhenUserIsParticipant()
        {
            var dbName = Guid.NewGuid().ToString();
            using var context = CreateInMemoryContext(dbName);

            context.ChatUsers.Add(new ChatUser { ChatId = 10, UserId = 1 });
            await context.SaveChangesAsync();

            var participationService = new ChatParticipationService(context);

            await participationService.EnsureUserIsParticipantAsync(10, 1);
        }

        [Fact]
        public async Task EnsureUserIsParticipantAsync_ThrowsForbiddenException_WhenUserIsNotParticipant()
        {
            var dbName = Guid.NewGuid().ToString();
            using var context = CreateInMemoryContext(dbName);

            var participationService = new ChatParticipationService(context);

            var exception = await Assert.ThrowsAsync<ForbiddenException>(
                () => participationService.EnsureUserIsParticipantAsync(20, 2));
            Assert.Equal("You are not a participant of this chat.", exception.Message);
        }
    }
}
