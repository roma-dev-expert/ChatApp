namespace ChatApp.Application.DTOs.Users
{
    public record RegisterRequest(string Username, string Password);
    public record LoginRequest(string Username, string Password);
    public record LoginResponse(string Token);
}
