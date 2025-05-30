namespace ChatApp.Domain.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; } // Password hash

        // Navigation property: User participates in multiple chats
        public ICollection<ChatUser> ChatUsers { get; set; } = new List<ChatUser>();
    }
}
