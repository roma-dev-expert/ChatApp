using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace ChatApp.Infrastructure.Services
{
    public class JwtService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<JwtService> _logger;

        public JwtService(IConfiguration configuration, ILogger<JwtService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public SymmetricSecurityKey GetSigningKey()
        {
            var jwtKey = _configuration["Jwt:Key"];
            if (string.IsNullOrEmpty(jwtKey))
            {
                _logger.LogError("JWT Key is missing in configuration. Authentication will fail.");
                throw new InvalidOperationException("JWT Key is missing from configuration.");
            }

            return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        }
    }
}
