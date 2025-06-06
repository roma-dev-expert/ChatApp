using ChatApp.Application.Interfaces;
using ChatApp.Domain.Entities;
using ChatApp.Domain.Exceptions;
using ChatApp.Infrastructure.Persistence;
using ChatApp.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ChatApp.UnitTests.Infrastructure.Services
{
    public class FakeChatParticipationService : IChatParticipationService
    {
        public Task EnsureUserIsParticipantAsync(int chatId, int userId)
        {
            return Task.CompletedTask;
        }
    }

    public class MessageServiceTests
    {
        private ApplicationDbContext CreateInMemoryContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task GetChatMessagesAsync_ReturnsPagedMessages_ForValidChat()
        {
            string dbName = Guid.NewGuid().ToString();
            using var context = CreateInMemoryContext(dbName);

            var chat = new Chat { Name = "Test Chat", ChatUsers = new List<ChatUser>() };
            chat.ChatUsers.Add(new ChatUser { ChatId = 1, UserId = 100, Chat = chat });
            context.Chats.Add(chat);
            await context.SaveChangesAsync();

            for (int i = 1; i <= 5; i++)
            {
                context.Messages.Add(new Message
                {
                    ChatId = chat.Id,
                    UserId = 100,
                    Text = $"Message {i}",
                    SentAt = DateTime.UtcNow.AddMinutes(i)
                });
            }
            await context.SaveChangesAsync();

            var messageService = new MessageService(context, new FakeChatParticipationService());

            var messagesPage1 = await messageService.GetChatMessagesAsync(chat.Id, 100, pageNumber: 1, pageSize: 3);

            Assert.NotNull(messagesPage1);
            var list1 = messagesPage1.ToList();
            Assert.Equal(3, list1.Count);
            Assert.Equal("Message 1", list1[0].Text);
            Assert.Equal("Message 2", list1[1].Text);
            Assert.Equal("Message 3", list1[2].Text);
        }

        [Fact]
        public async Task GetMessageByIdAsync_ReturnsMessageDto_WhenMessageExistsAndUserIsParticipant()
        {
            string dbName = Guid.NewGuid().ToString();
            using var context = CreateInMemoryContext(dbName);
            var chat = new Chat { Name = "Chat A", ChatUsers = new List<ChatUser>() };
            chat.ChatUsers.Add(new ChatUser { ChatId = 1, UserId = 200, Chat = chat });
            context.Chats.Add(chat);
            await context.SaveChangesAsync();

            var message = new Message
            {
                ChatId = chat.Id,
                UserId = 200,
                Text = "Hello World",
                SentAt = DateTime.UtcNow
            };
            context.Messages.Add(message);
            await context.SaveChangesAsync();

            var messageService = new MessageService(context, new FakeChatParticipationService());

            var result = await messageService.GetMessageByIdAsync(chat.Id, message.Id, 200);

            Assert.NotNull(result);
            Assert.Equal("Hello World", result.Text);
        }

        [Fact]
        public async Task GetMessageByIdAsync_ReturnsNull_WhenMessageDoesNotExist()
        {
            string dbName = Guid.NewGuid().ToString();
            using var context = CreateInMemoryContext(dbName);
            var chat = new Chat { Name = "Chat B", ChatUsers = new List<ChatUser>() };
            chat.ChatUsers.Add(new ChatUser { ChatId = 1, UserId = 300, Chat = chat });
            context.Chats.Add(chat);
            await context.SaveChangesAsync();

            var messageService = new MessageService(context, new FakeChatParticipationService());

            var result = await messageService.GetMessageByIdAsync(chat.Id, 999, 300);

            Assert.Null(result);
        }

        [Fact]
        public async Task SearchMessagesAsync_ReturnsMessagesContainingKeyword()
        {
            string dbName = Guid.NewGuid().ToString();
            using var context = CreateInMemoryContext(dbName);

            var chat1 = new Chat { Name = "Chat 1", ChatUsers = new List<ChatUser>() };
            chat1.ChatUsers.Add(new ChatUser { ChatId = 1, UserId = 400, Chat = chat1 });
            var chat2 = new Chat { Name = "Chat 2", ChatUsers = new List<ChatUser>() };
            chat2.ChatUsers.Add(new ChatUser { ChatId = 2, UserId = 400, Chat = chat2 });
            context.Chats.AddRange(chat1, chat2);
            await context.SaveChangesAsync();

            context.Messages.AddRange(new List<Message>
            {
                new Message { ChatId = chat1.Id, UserId = 400, Text = "Hello world", SentAt = DateTime.UtcNow.AddMinutes(1) },
                new Message { ChatId = chat1.Id, UserId = 400, Text = "Another message", SentAt = DateTime.UtcNow.AddMinutes(2) },
                new Message { ChatId = chat2.Id, UserId = 400, Text = "Hello again", SentAt = DateTime.UtcNow.AddMinutes(3) },
                new Message { ChatId = chat2.Id, UserId = 400, Text = "Irrelevant", SentAt = DateTime.UtcNow.AddMinutes(4) }
            });
            await context.SaveChangesAsync();

            var messageService = new MessageService(context, new FakeChatParticipationService());

            var results = await messageService.SearchMessagesAsync(400, "Hello", pageNumber: 1, pageSize: 10);

            var list = results.ToList();
            Assert.Equal(2, list.Count);
            Assert.All(list, m => Assert.Contains("Hello", m.Text));
        }

        [Fact]
        public async Task SearchMessagesByChatAsync_ReturnsMessagesContainingKeyword_ForSpecifiedChat()
        {
            string dbName = Guid.NewGuid().ToString();
            using var context = CreateInMemoryContext(dbName);
            var chat = new Chat { Name = "Chat X", ChatUsers = new List<ChatUser>() };
            chat.ChatUsers.Add(new ChatUser { ChatId = 1, UserId = 500, Chat = chat });
            context.Chats.Add(chat);
            await context.SaveChangesAsync();

            context.Messages.AddRange(new List<Message>
            {
                new Message { ChatId = chat.Id, UserId = 500, Text = "Keyword: test", SentAt = DateTime.UtcNow.AddMinutes(1) },
                new Message { ChatId = chat.Id, UserId = 500, Text = "No match here", SentAt = DateTime.UtcNow.AddMinutes(2) },
                new Message { ChatId = chat.Id, UserId = 500, Text = "Another test message", SentAt = DateTime.UtcNow.AddMinutes(3) },
            });
            await context.SaveChangesAsync();

            var messageService = new MessageService(context, new FakeChatParticipationService());

            var results = await messageService.SearchMessagesByChatAsync(chat.Id, 500, "test", pageNumber: 1, pageSize: 10);

            var list = results.ToList();
            Assert.Equal(2, list.Count);
            Assert.All(list, m => Assert.Contains("test", m.Text, StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public async Task SendMessageAsync_CreatesMessageAndReturnsDto()
        {
            string dbName = Guid.NewGuid().ToString();
            using var context = CreateInMemoryContext(dbName);
            var chat = new Chat { Name = "Chat Send", ChatUsers = new List<ChatUser>() };
            chat.ChatUsers.Add(new ChatUser { ChatId = 1, UserId = 600, Chat = chat });
            context.Chats.Add(chat);
            await context.SaveChangesAsync();

            var messageService = new MessageService(context, new FakeChatParticipationService());

            var result = await messageService.SendMessageAsync(chat.Id, 600, "Test message");

            Assert.NotNull(result);
            Assert.Equal("Test message", result.Text);

            var messageEntity = await context.Messages.FirstOrDefaultAsync(m => m.Id == result.Id);
            Assert.NotNull(messageEntity);
            Assert.Equal(600, messageEntity.UserId);
        }

        [Fact]
        public async Task SendMessageAsync_ThrowsArgumentException_WhenTextIsEmpty()
        {
            string dbName = Guid.NewGuid().ToString();
            using var context = CreateInMemoryContext(dbName);

            var chat = new Chat { Name = "Chat Invalid", ChatUsers = new List<ChatUser>() };
            chat.ChatUsers.Add(new ChatUser { ChatId = 1, UserId = 700, Chat = chat });
            context.Chats.Add(chat);
            await context.SaveChangesAsync();

            var messageService = new MessageService(context, new FakeChatParticipationService());

            await Assert.ThrowsAsync<ArgumentException>(() => messageService.SendMessageAsync(chat.Id, 700, "   "));
        }

        [Fact]
        public async Task DeleteMessageAsync_DeletesMessage_WhenUserIsAuthorized()
        {
            string dbName = Guid.NewGuid().ToString();
            using var context = CreateInMemoryContext(dbName);
            var chat = new Chat { Name = "Chat Delete", ChatUsers = new List<ChatUser>() };
            chat.ChatUsers.Add(new ChatUser { ChatId = 1, UserId = 800, Chat = chat });
            context.Chats.Add(chat);
            await context.SaveChangesAsync();

            var message = new Message
            {
                ChatId = chat.Id,
                UserId = 800,
                Text = "Message to delete",
                SentAt = DateTime.UtcNow
            };
            context.Messages.Add(message);
            await context.SaveChangesAsync();

            var messageService = new MessageService(context, new FakeChatParticipationService());

            await messageService.DeleteMessageAsync(chat.Id, message.Id, 800);

            var foundMessage = await context.Messages.FirstOrDefaultAsync(m => m.Id == message.Id);
            Assert.Null(foundMessage);
        }

        [Fact]
        public async Task DeleteMessageAsync_ThrowsForbiddenException_WhenUserIsNotOwner()
        {
            string dbName = Guid.NewGuid().ToString();
            using var context = CreateInMemoryContext(dbName);
            var chat = new Chat { Name = "Chat Delete Not Owner", ChatUsers = new List<ChatUser>() };
            chat.ChatUsers.Add(new ChatUser { ChatId = 1, UserId = 900, Chat = chat });
            context.Chats.Add(chat);
            await context.SaveChangesAsync();

            var message = new Message
            {
                ChatId = chat.Id,
                UserId = 901,
                Text = "Message to delete",
                SentAt = DateTime.UtcNow
            };
            context.Messages.Add(message);
            await context.SaveChangesAsync();

            var messageService = new MessageService(context, new FakeChatParticipationService());

            await Assert.ThrowsAsync<ForbiddenException>(() => messageService.DeleteMessageAsync(chat.Id, message.Id, 900));
        }

        [Fact]
        public async Task EditMessageAsync_UpdatesMessageText_WhenUserIsAuthorized()
        {
            string dbName = Guid.NewGuid().ToString();
            using var context = CreateInMemoryContext(dbName);
            var chat = new Chat { Name = "Chat Edit", ChatUsers = new List<ChatUser>() };
            chat.ChatUsers.Add(new ChatUser { ChatId = 1, UserId = 1000, Chat = chat });
            context.Chats.Add(chat);
            await context.SaveChangesAsync();

            var message = new Message
            {
                ChatId = chat.Id,
                UserId = 1000,
                Text = "Old message",
                SentAt = DateTime.UtcNow
            };
            context.Messages.Add(message);
            await context.SaveChangesAsync();

            var messageService = new MessageService(context, new FakeChatParticipationService());
            string newText = "Updated message";

            var updatedDto = await messageService.EditMessageAsync(chat.Id, message.Id, 1000, newText);

            Assert.NotNull(updatedDto);
            Assert.Equal(newText, updatedDto.Text);

            var updatedMsg = await context.Messages.FirstOrDefaultAsync(m => m.Id == message.Id);
            Assert.NotNull(updatedMsg);
            Assert.Equal(newText, updatedMsg.Text);
        }

        [Fact]
        public async Task EditMessageAsync_ThrowsArgumentException_WhenNewTextIsEmpty()
        {
            string dbName = Guid.NewGuid().ToString();
            using var context = CreateInMemoryContext(dbName);
            var chat = new Chat { Name = "Chat Edit Invalid", ChatUsers = new List<ChatUser>() };
            chat.ChatUsers.Add(new ChatUser { ChatId = 1, UserId = 1100, Chat = chat });
            context.Chats.Add(chat);
            await context.SaveChangesAsync();

            var message = new Message
            {
                ChatId = chat.Id,
                UserId = 1100,
                Text = "Original message",
                SentAt = DateTime.UtcNow
            };
            context.Messages.Add(message);
            await context.SaveChangesAsync();

            var messageService = new MessageService(context, new FakeChatParticipationService());

            await Assert.ThrowsAsync<ArgumentException>(() =>
                messageService.EditMessageAsync(chat.Id, message.Id, 1100, "   "));
        }
    }
}
