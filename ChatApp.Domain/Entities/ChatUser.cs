namespace ChatApp.Domain.Entities
{
    public class ChatUser
    {
        public int ChatId { get; set; }
        public required Chat Chat { get; set; }

        public int UserId { get; set; }
        public required User User { get; set; }
    }
}
