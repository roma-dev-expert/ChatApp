namespace ChatApp.Application.DTOs
{
    public class ChatUserDto
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public List<int> ParticipantIds { get; set; } = new();
    }
}
