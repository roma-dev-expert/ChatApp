using ChatApp.Domain.Entities;
using ChatApp.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ChatApp.Api.Controllers
{
    [ApiController]
    [Authorize]
    public abstract class BaseController : ControllerBase
    {
        protected readonly ApplicationDbContext _context;

        public BaseController(ApplicationDbContext context)
        {
            _context = context;
        }

        protected async Task<User> GetCurrentUserAsync()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)
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
