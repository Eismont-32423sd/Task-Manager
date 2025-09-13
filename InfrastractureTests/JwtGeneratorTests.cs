using Domain.Entities.DbEntities;
using Domain.Entities.Enums;
using Infrastracture.Services.JWT;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.JsonWebTokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using JwtRegisteredClaimNames =
    Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;


namespace InfrastractureTests
{
    public class JwtGeneratorTests
    {
        private readonly JwtGenerator _jwtGenerator;
        private readonly IConfiguration _configuration;
        private readonly User _testUser;

        public JwtGeneratorTests()
        {
            var inMemorySettings = new Dictionary<string, string>
            {
                {"Jwt:secretKey", "ThisIsAVerySecureKeyForTestingWhichIsAtLeast16BytesLong"},
                {"Jwt:Expiration", "60"},
                {"Jwt:Issuer", "TestIssuer"},
                {"Jwt:Audience", "TestAudience"}
            };

            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            _jwtGenerator = new JwtGenerator(_configuration);

            _testUser = new User
            {
                Id = Guid.NewGuid(),
                Email = "testuser@example.com",
                UserName = "testuser",
                Role = Role.Manager,
                PasswordHash = "hashedpassword"
            };
        }

        [Fact]
        public void CreateJwtToken_Generates_Valid_Token()
        {
            string token = _jwtGenerator.CreateJwtToken(_testUser);

            Assert.NotNull(token);
            Assert.NotEmpty(token);
            Assert.NotEqual("", token);
        }

        [Fact]
        public void CreateJwtToken_Contains_Correct_Claims()
        {
            var tokenHandler = new JsonWebTokenHandler();

            string tokenString = _jwtGenerator.CreateJwtToken(_testUser);
            var securityToken = tokenHandler.ReadJsonWebToken(tokenString);
            var claims = securityToken.Claims.ToList();

            Assert.Contains(claims, c => c.Type == 
                JwtRegisteredClaimNames.Sub && c.Value == _testUser.Id.ToString());
            Assert.Contains(claims, c => c.Type == 
                JwtRegisteredClaimNames.Email && c.Value == _testUser.Email);
            Assert.Contains(claims, c => c.Type == 
                ClaimTypes.Role && c.Value == _testUser.Role.ToString());
        }

        [Fact]
        public void CreateJwtToken_Contains_Correct_Expiration()
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var expirationMinutes = Convert.ToDouble(_configuration["Jwt:Expiration"]);

            string tokenString = _jwtGenerator.CreateJwtToken(_testUser);
            var securityToken = tokenHandler.ReadToken(tokenString) as JwtSecurityToken;

            var expectedExpiration = DateTime.UtcNow.AddMinutes(expirationMinutes);
            var actualExpiration = securityToken.ValidTo;

            Assert.True(actualExpiration >= expectedExpiration.AddSeconds(-5));
            Assert.True(actualExpiration <= expectedExpiration.AddSeconds(5));
        }

        [Fact]
        public void CreateJwtToken_Contains_Correct_Audience_And_Issuer()
        {
            var tokenHandler = new JsonWebTokenHandler();

            string tokenString = _jwtGenerator.CreateJwtToken(_testUser);
            var securityToken = tokenHandler.ReadJsonWebToken(tokenString);

            Assert.Equal(_configuration["Jwt:Issuer"], securityToken.Issuer);
            Assert.Single(securityToken.Audiences, _configuration["Jwt:Audience"]);
        }
    }
}