using Application.Services.DTOs.AuthenticationDTOS;
using Application.Services.DTOs.Generic;
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

        public async Task<ServiceResult<string>>
            LoginAsync(LoginRequest loginRequest)
        {
            using (LogContext.PushProperty("Operation", nameof(LoginAsync)))
            {
                var user = await _unitOfWork.UserRepository
                    .GetByUserNameAsync(loginRequest.UserName!);

                if (user == null)
                {
                    _logger.LogError("Inavalid username");
                    return new ServiceResult<string>
                    {
                        IsSucceded = false,
                        Errors = new[] { "User with such username doesn`t exist" },
                        Message = "Conflict",
                        Data = null
                    };
                }

                if (!_passwordHasher.Verify(loginRequest.Password!, user.PasswordHash!))
                {
                    _logger.LogError("Password doesn`t match");
                    return new ServiceResult<string>
                    {
                        IsSucceded = false,
                        Errors = new[] { "Password doesn`t match" },
                        Message = "Conflict",
                        Data = null
                    };
                }

                if (!user.IsConfirmed)
                {
                    _logger.LogError("User didn`t confirm credeantials");
                    return new ServiceResult<string>
                    {
                        IsSucceded = false,
                        Errors = new[] { "Some error ocured" },
                        Message = "Conflict",
                        Data = null
                    };
                }

                var verificationToken = _jwtGenerator.CreateJwtToken(user);
                user.ConfirmationToken = verificationToken;
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("User logged in");
                return new ServiceResult<string>
                {
                    IsSucceded = true,
                    Errors = null,
                    Message = "User logged in succesfully",
                    Data = verificationToken
                };
            }
        }
    }
}
