using ChatApp.Application.Interfaces;
using ChatApp.Domain.Entities;
using ChatApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ChatApp.Infrastructure.Services
{
    public class UserContext : IUserContext
    {
        private readonly ApplicationDbContext _context;

        public UserContext(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<User> GetCurrentUserAsync(ClaimsPrincipal userClaims)
        {
            var userIdClaim = userClaims.FindFirst(ClaimTypes.NameIdentifier)
                              ?? throw new UnauthorizedAccessException("User ID is missing in token.");

            if (!int.TryParse(userIdClaim.Value, out int userId))
            {
                throw new ArgumentException("Invalid user ID format.");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId)
                       ?? throw new KeyNotFoundException("User not found in the database.");

            return user;
        }
    }
}
