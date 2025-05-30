namespace ChatApp.Domain.Entities
{
    public class Chat
    {
        public int Id { get; set; }
        public string Name { get; set; } // Chat name

        // Navigation property: Chat has multiple participants
        public ICollection<ChatUser> ChatUsers { get; set; } = new List<ChatUser>();

        // Navigation property: Chat contains many messages
        public ICollection<Message> Messages { get; set; } = new List<Message>();
    }
}
