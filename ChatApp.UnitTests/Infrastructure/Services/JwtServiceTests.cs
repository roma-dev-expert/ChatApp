using System.Text;
using ChatApp.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace ChatApp.UnitTests.Infrastructure.Services
{
    public class JwtServiceTests
    {
        private IConfiguration BuildConfigurationWithKey(string jwtKey)
        {
            var configData = new Dictionary<string, string?>
            {
                { "Jwt:Key", jwtKey }
            };
            return new ConfigurationBuilder()
                .AddInMemoryCollection(configData)
                .Build();
        }

        [Fact]
        public void GetSigningKey_ReturnsSymmetricSecurityKey_WhenKeyExists()
        {
            string testKey = "SuperSecretTestKey1234";
            var configuration = BuildConfigurationWithKey(testKey);
            ILogger<JwtService> logger = NullLogger<JwtService>.Instance;
            var jwtService = new JwtService(configuration, logger);

            SymmetricSecurityKey securityKey = jwtService.GetSigningKey();

            Assert.NotNull(securityKey);
            var expectedBytes = Encoding.UTF8.GetBytes(testKey);
            Assert.Equal(expectedBytes, securityKey.Key);
        }

        [Fact]
        public void GetSigningKey_ThrowsInvalidOperationException_WhenKeyMissing()
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>())
                .Build();
            ILogger<JwtService> logger = NullLogger<JwtService>.Instance;
            var jwtService = new JwtService(configuration, logger);

            var exception = Assert.Throws<InvalidOperationException>(() => jwtService.GetSigningKey());
            Assert.Equal("JWT Key is missing from configuration.", exception.Message);
        }
    }
}
