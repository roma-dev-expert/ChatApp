namespace ChatApp.Domain.Entities
{
    public class Message
    {
        public int Id { get; set; }

        // Foreign key: Message belongs to a specific chat
        public int ChatId { get; set; }
        public Chat Chat { get; set; }

        // Foreign key: Message sent by a specific user
        public int UserId { get; set; }
        public User User { get; set; }

        public string Text { get; set; } // Message content

        public DateTime SentAt { get; set; } // Timestamp when the message was sent
    }
}
