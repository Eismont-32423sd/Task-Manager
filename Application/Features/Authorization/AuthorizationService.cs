using Application.Services.DTOs.AuthenticationDTOS;
using Application.Services.Interfaces;
using Domain.Abstractions;

namespace Application.Features.Authorization
{
    public class AuthorizationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtGenerator _jwtGenerator;

        public AuthorizationService(IUnitOfWork unitOfWork,
            IPasswordHasher passwordHasher,
            IJwtGenerator jwtGenerator)
        {
            _unitOfWork = unitOfWork;
            _passwordHasher = passwordHasher;
            _jwtGenerator = jwtGenerator;
        }

        public async Task<(bool isSucceded, IEnumerable<string> errors, string message, string? token)>
            LoginAsync(LoginRequest loginRequest)
        {
            var user = await _unitOfWork.UserRepository
                .GetByUserNameAsync(loginRequest.UserName!);

            if (user == null)
            {
                return (false, new[] { "User with such user name doesn`t exist" }, "Conflict", null);
            }

            if (!_passwordHasher.Verify(loginRequest.Password!, user.PasswordHash!))
            {
                return (false, new[] { "Password doesn`t match" }, "Conflict", null);
            }
            if (!user.IsConfirmed)
            {
                return (false, new[] { "You did`nt confirm your data, please check your email box" }, "Conflict", null);
            }

            var verificationToken = _jwtGenerator.CreateJwtToken(user);

            return (true, null, "Logged in succesfully", verificationToken);
        }
    }
}
