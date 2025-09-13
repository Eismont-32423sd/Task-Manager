using Infrastracture.Services.PasswordHash;

namespace InfrastractureTests
{
    public class PasswordHasherTests
    {
        private readonly PasswordHasher _passwordHasher;

        public PasswordHasherTests()
        {
            _passwordHasher = new PasswordHasher();
        }

        [Fact]
        public void Hash_ShouldReturnValidFormat()
        {
            string password = "mySecurePassword123";

            string hashedPassword = _passwordHasher.Hash(password);

            Assert.NotNull(hashedPassword);
            Assert.NotEmpty(hashedPassword);

            var parts = hashedPassword.Split('-');
            Assert.Equal(2, parts.Length);

            Assert.True(IsHexString(parts[0]));
            Assert.True(IsHexString(parts[1]));
        }

        [Theory]
        [InlineData("CorrectPassword", "CorrectPassword", true)]
        [InlineData("CorrectPassword", "IncorrectPassword", false)]
        public void Verify_ShouldReturnExpectedResult(string passwordToHash, string passwordToVerify, bool expectedResult)
        {
            string hashedPassword = _passwordHasher.Hash(passwordToHash);

            bool result = _passwordHasher.Verify(passwordToVerify, hashedPassword);

            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void Verify_WithInvalidFormat_ShouldReturnFalse()
        {
            string password = "testpassword";
            string invalidHashedPassword = "invalid-format";

            bool result = _passwordHasher.Verify(password, invalidHashedPassword);

            Assert.False(result);
        }

        private bool IsHexString(string input)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(input, @"\A\b[0-9a-fA-F]+\b\Z");
        }
    }
}
