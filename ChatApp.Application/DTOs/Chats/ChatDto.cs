using ChatApp.Domain.Entities;

namespace ChatApp.Application.DTOs.Chats
{
    public class ChatDto
    {
        public int Id { get; set; }
        public required string Name { get; set; }
    }
}
