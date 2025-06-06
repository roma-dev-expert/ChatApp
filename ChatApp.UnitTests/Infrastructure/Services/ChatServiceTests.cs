using ChatApp.Domain.Entities;
using ChatApp.Infrastructure.Persistence;
using ChatApp.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ChatApp.UnitTests.Infrastructure.Services
{
    public class ChatServiceTests
    {
        private ApplicationDbContext CreateInMemoryContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: dbName) 
                .Options;
            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task GetUserChatsAsync_ReturnsChats_ForUserParticipants()
        {
            string dbName = Guid.NewGuid().ToString();
            using var context = CreateInMemoryContext(dbName);

            var chat1 = new Chat
            {
                Name = "Chat 1",
                ChatUsers = new List<ChatUser>()
            };
            var chat2 = new Chat
            {
                Name = "Chat 2",
                ChatUsers = new List<ChatUser>()
            };

            chat1.ChatUsers.Add(new ChatUser { UserId = 1, Chat = chat1 });

            context.Chats.Add(chat1);
            context.Chats.Add(chat2);
            await context.SaveChangesAsync();

            var chatService = new ChatService(context);

            var result = await chatService.GetUserChatsAsync(1);

            Assert.NotNull(result);
            var chatList = result.ToList();
            Assert.Single(chatList);
            Assert.Equal("Chat 1", chatList[0].Name);
        }

        [Fact]
        public async Task GetChatByIdAsync_ReturnsChat_ForUserParticipant()
        {
            string dbName = Guid.NewGuid().ToString();
            using var context = CreateInMemoryContext(dbName);

            var chat = new Chat
            {
                Name = "Chat A",
                ChatUsers = new List<ChatUser>()
            };
            chat.ChatUsers.Add(new ChatUser { UserId = 1, Chat = chat });
            context.Chats.Add(chat);
            await context.SaveChangesAsync();

            int chatId = chat.Id;

            var chatService = new ChatService(context);

            var result = await chatService.GetChatByIdAsync(1, chatId);

            Assert.NotNull(result);
            Assert.Equal("Chat A", result.Name);
        }

        [Fact]
        public async Task GetChatByIdAsync_ReturnsNull_WhenUserNotParticipant()
        {
            string dbName = Guid.NewGuid().ToString();
            using var context = CreateInMemoryContext(dbName);

            var chat = new Chat
            {
                Name = "Chat B",
                ChatUsers = new List<ChatUser>()
            };
            chat.ChatUsers.Add(new ChatUser { UserId = 2, Chat = chat });
            context.Chats.Add(chat);
            await context.SaveChangesAsync();

            var chatService = new ChatService(context);

            var result = await chatService.GetChatByIdAsync(1, chat.Id);

            Assert.Null(result);
        }

        [Fact]
        public async Task CreateChatAsync_CreatesChatAndAddsUserAsParticipant()
        {
            string dbName = Guid.NewGuid().ToString();
            using var context = CreateInMemoryContext(dbName);

            var chatService = new ChatService(context);

            var result = await chatService.CreateChatAsync(1, "New Chat");

            Assert.NotNull(result);
            Assert.Equal("New Chat", result.Name);

            var chatEntity = await context.Chats.Include(c => c.ChatUsers)
                .FirstOrDefaultAsync(c => c.Id == result.Id);
            Assert.NotNull(chatEntity);
            Assert.Equal("New Chat", chatEntity.Name);
            Assert.NotEmpty(chatEntity.ChatUsers);
            Assert.Contains(chatEntity.ChatUsers, cu => cu.UserId == 1);
        }

        [Fact]
        public async Task CreateChatAsync_ThrowsArgumentException_WhenChatNameIsInvalid()
        {
            string dbName = Guid.NewGuid().ToString();
            using var context = CreateInMemoryContext(dbName);

            var chatService = new ChatService(context);

            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => chatService.CreateChatAsync(1, "    "));

            Assert.Contains("Chat name must be provided", exception.Message);
        }
    }
}
