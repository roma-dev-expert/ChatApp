using ChatApp.Application.Interfaces;
using ChatApp.Domain.Entities;
using ChatApp.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChatApp.Api.Controllers
{
    [ApiController]
    [Authorize]
    public abstract class BaseController : ControllerBase
    {
        protected readonly ApplicationDbContext _context;
        protected readonly IUserContext _userContext;

        public BaseController(ApplicationDbContext context, IUserContext userContext)
        {
            _context = context;
            _userContext = userContext;
        }

        protected Task<User> GetCurrentUserAsync()
        {
            return _userContext.GetCurrentUserAsync(User);
        }
    }
}
