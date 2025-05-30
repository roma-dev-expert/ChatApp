namespace ChatApp.Domain.Entities
{
    public class ChatUser
    {
        // Composite key: ChatId plus UserId uniquely identify the relation
        public int ChatId { get; set; }
        public Chat Chat { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }
    }
}
