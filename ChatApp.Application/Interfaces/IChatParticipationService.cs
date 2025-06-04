namespace ChatApp.Application.Interfaces
{
    public interface IChatParticipationService
    {
        Task EnsureUserIsParticipantAsync(int chatId, int userId);
    }
}
