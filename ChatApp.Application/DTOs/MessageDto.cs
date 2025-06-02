namespace ChatApp.Application.DTOs
{
    public class MessageDto
    {
        public int Id { get; set; }
        public int ChatId { get; set; }
        public int UserId { get; set; }
        public required string Text { get; set; }
        public DateTime SentAt { get; set; }
    }
}
