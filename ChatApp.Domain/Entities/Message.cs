namespace ChatApp.Domain.Entities
{
    public class Message
    {
        public int Id { get; set; }

        public int ChatId { get; set; }
        public required Chat Chat { get; set; }

        public int UserId { get; set; }
        public required User User { get; set; }

        public required string Text { get; set; }

        public DateTime SentAt { get; set; }
    }
}
