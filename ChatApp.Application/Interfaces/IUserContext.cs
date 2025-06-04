using ChatApp.Domain.Entities;
using System.Security.Claims;

namespace ChatApp.Application.Interfaces
{
    public interface IUserContext
    {
        Task<User> GetCurrentUserAsync(ClaimsPrincipal userClaims);
    }
}
