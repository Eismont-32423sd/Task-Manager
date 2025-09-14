using Application.Services.DTOs.AuthenticationDTOS;
using Application.Services.DTOs.Generic;
using Application.Services.Interfaces;
using Domain.Abstractions;
using Microsoft.Extensions.Logging;
using Serilog.Context;

namespace Application.Features.Authentication
{
    public class AuthenticationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ILogger<AuthenticationService> _logger;
        private readonly IJwtGenerator _jwtGenerator;
        public AuthenticationService(IUnitOfWork unitOfWork,
                              IPasswordHasher passwordHasher,
                              ILogger<AuthenticationService> logger,
                              IJwtGenerator jwtGenerator)
        {
            _unitOfWork = unitOfWork;
            _passwordHasher = passwordHasher;
            _logger = logger;
            _jwtGenerator = jwtGenerator;
        }

        public async Task<ServiceResult<object>>
            RegisterAsync(RegisterRequest registerRequest, string confirmationLinkBaseUrl)
        {
            using (LogContext.PushProperty("Operation", nameof(RegisterAsync)))
            {
                var existingUserbyEmail = await _unitOfWork
                    .UserRepository.GetByEmailAsync(registerRequest.Email!);
                var existingUserByUserName = await _unitOfWork
                    .UserRepository.GetByUserNameAsync(registerRequest.UserName!);

                if (existingUserbyEmail != null)
                {
                    _logger.LogError($"User with such email already exists, " +
                        $"{existingUserbyEmail.Email}");
                    return new ServiceResult<object>
                    {
                        IsSucceded = false,
                        Errors = new[] { $"User with such email already exists, " +
                        $"{existingUserbyEmail.Email}" },
                        Message = "Conflict",
                        Data = null
                    };
                }

                if (existingUserByUserName != null)
                {
                    _logger.LogError($"User with such username already exists, " +
                        $"{existingUserByUserName.UserName}");
                    return new ServiceResult<object>
                    {
                        IsSucceded = false,
                        Errors = new[] { $"User with such username already exists, " +
                        $"{existingUserByUserName.UserName}" },
                        Message = "Conflict",
                        Data = null
                    };
                }

                var user = new Domain.Entities.DbEntities.User
                {
                    Id = Guid.NewGuid(),
                    UserName = registerRequest.UserName,
                    Email = registerRequest.Email,
                    PasswordHash = _passwordHasher.Hash(registerRequest.Password!),
                    IsConfirmed = true
                };

                var confirmationToken = _jwtGenerator.CreateJwtToken(user);
                user.ConfirmationToken = confirmationToken;

                await _unitOfWork.UserRepository.AddAsync(user);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("User registered succesfully");
                return new ServiceResult<object>
                {
                    IsSucceded = true,
                    Errors = null,
                    Message = "User registered succesfully",
                    Data = null
                };
            }
        }
    }
}
