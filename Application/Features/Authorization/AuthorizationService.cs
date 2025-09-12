using Application.Services.DTOs.AuthenticationDTOS;
using Application.Services.Interfaces;
using Domain.Abstractions;
using Microsoft.Extensions.Logging;
using Serilog.Context;

namespace Application.Features.Authorization
{
    public class AuthorizationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtGenerator _jwtGenerator;
        private readonly ILogger<AuthorizationService> _logger;
        public AuthorizationService(IUnitOfWork unitOfWork,
            IPasswordHasher passwordHasher,
            IJwtGenerator jwtGenerator,
            ILogger<AuthorizationService> logger)
        {
            _unitOfWork = unitOfWork;
            _passwordHasher = passwordHasher;
            _jwtGenerator = jwtGenerator;
            _logger = logger;
        }

        public async Task<(bool isSucceded, IEnumerable<string> errors, string message, string? token)>
            LoginAsync(LoginRequest loginRequest)
        {
            using (LogContext.PushProperty("Operation", nameof(LoginAsync)))
            {
                var user = await _unitOfWork.UserRepository
                    .GetByUserNameAsync(loginRequest.UserName!);

                if (user == null)
                {
                    _logger.LogError("Inavalid username");
                    return (false, new[] { "User with such username doesn`t exist" }, "Conflict", null);
                }

                if (!_passwordHasher.Verify(loginRequest.Password!, user.PasswordHash!))
                {
                    _logger.LogError("Password doesn`t match");
                    return (false, new[] { "Password doesn`t match" }, "Conflict", null);
                }

                if (!user.IsConfirmed)
                {
                    _logger.LogError("User didn`t confirm credeantials");
                    return (false, new[] { "You did`nt confirm your data, please check your email box" }, "Conflict", null);
                }

                var verificationToken = _jwtGenerator.CreateJwtToken(user);

                _logger.LogInformation("User logged in");
                return (true, null, "Logged in succesfully", verificationToken);
            }
        }
    }
}
