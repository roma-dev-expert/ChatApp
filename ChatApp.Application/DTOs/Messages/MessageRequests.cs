namespace ChatApp.Application.DTOs.Messages
{
    public record CreateMessageRequest(string Text);
    public record EditMessageRequest(string Text);
}