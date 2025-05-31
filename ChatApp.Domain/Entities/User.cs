namespace ChatApp.Domain.Entities
{
    public class User
    {
        public int Id { get; set; }
        public required string Username { get; set; }
        public required string PasswordHash { get; set; }
        public ICollection<ChatUser> ChatUsers { get; set; } = new List<ChatUser>();
    }
}
