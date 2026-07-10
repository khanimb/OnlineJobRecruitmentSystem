using Microsoft.Extensions.Configuration;
using OnlineJobRecruitmentSystem.Domain.Entities;
using OnlineJobRecruitmentSystem.Infrastructure.Services;
using System.IdentityModel.Tokens.Jwt;

namespace OnlineJobRecruitmentSystem.Tests.Services
{
    public class JwtServiceTests
    {
        private readonly JwtService _jwtService;

        public JwtServiceTests()
        {
            var configValues = new Dictionary<string, string?>
            {
                { "Jwt:Key", "TestSecretKeyForUnitTestsOnly12345!" },
                { "Jwt:Issuer", "TestIssuer" },
                { "Jwt:Audience", "TestAudience" }
            };

            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configValues)
                .Build();

            _jwtService = new JwtService(configuration);
        }

        [Fact]
        public void GenerateToken_ReturnsValidJwtWithCorrectClaims()
        {
            var user = new User { Id = 42, Email = "user@test.com", Role = "JobSeeker", Username = "testuser" };

            var token = _jwtService.GenerateToken(user);
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);

            var claimValues = jwt.Claims.Select(c => c.Value).ToList();

            Assert.Contains("42", claimValues);
            Assert.Contains("JobSeeker", claimValues);
        }

        [Fact]
        public void GenerateRefreshToken_ReturnsNonEmptyUniqueValues()
        {
            var token1 = _jwtService.GenerateRefreshToken();
            var token2 = _jwtService.GenerateRefreshToken();

            Assert.False(string.IsNullOrWhiteSpace(token1));
            Assert.NotEqual(token1, token2);
        }
    }
}