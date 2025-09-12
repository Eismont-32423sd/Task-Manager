using Application.Services.DTOs.AuthenticationDTOS;
using Application.Services.Interfaces;
using Domain.Abstractions;
using Domain.Entities;
using FluentEmail.Core;
using Microsoft.Extensions.Logging;
using Serilog.Context;

namespace Application.Features.Authentication
{
    public class AuthenticationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtGenerator _jwtGenerator;
        private readonly IFluentEmail _emailFactory;
        private readonly ILogger<AuthenticationService> _logger;
        public AuthenticationService(IUnitOfWork unitOfWork,
                              IPasswordHasher passwordHasher,
                              IJwtGenerator jwtGenerator,
                              IFluentEmail emailFactory,
                              ILogger<AuthenticationService> logger)
        {
            _unitOfWork = unitOfWork;
            _passwordHasher = passwordHasher;
            _jwtGenerator = jwtGenerator;
            _emailFactory = emailFactory;
            _logger = logger;
        }

        public async Task<(bool isSucceded, IEnumerable<string>? errors,
            string message, string? ConfiramationToken, Guid? UserId)>
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
                    _logger.LogError($"User with such email already exists, {existingUserbyEmail.Email}");
                    return (false, new[] { "User with such email already exists" }, "Conflict", null, null);
                }

                if (existingUserByUserName != null)
                {
                    _logger.LogError($"User with such username already exists, {existingUserByUserName.UserName}");
                    return (false, new[] { "User with such user name already exists" }, "Conflict", null, null);
                }

                var user = new Domain.Entities.User
                {
                    Id = Guid.NewGuid(),
                    UserName = registerRequest.UserName,
                    Email = registerRequest.Email,
                    PasswordHash = _passwordHasher.Hash(registerRequest.Password!),
                    IsConfirmed = false
                };
                //var adminUser = new User
                //{
                //    Id= Guid.NewGuid(),
                //    UserName = "Admin",
                //    Email = "Admin@gmail.com",
                //    PasswordHash = _passwordHasher.Hash("strong_admin_pass_123"),
                //    IsConfirmed = true,
                //    Role = Role.TeamLead
                //};
                //await _unitOfWork.UserRepository.AddAsync(adminUser);
                //await _unitOfWork.SaveChangesAsync();

                var confirmationToken = _jwtGenerator.CreateJwtToken(user);
                user.ConfirmationToken = confirmationToken;

                await _unitOfWork.UserRepository.AddAsync(user);
                await _unitOfWork.SaveChangesAsync();

                var confirmationLink = $"{confirmationLinkBaseUrl}?userId={user.Id}&token={Uri.EscapeDataString(confirmationToken)}";

                try
                {
                    _logger.LogInformation("Attemp to send verification email");
                    await _emailFactory.To(user.Email)
                        .Subject("Email verification for Task Manager")
                        .Body($"To verify you email <a href='{confirmationLink}'>click here</a>", isHtml: true)
                        .SendAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError("Failed to send confirmation email.");
                    return (false, new[] { "Failed to send confirmation email." },
                        $"User registered, but failed to send confirmation email. {ex.Message}", null, null);
                }

                _logger.LogInformation("User registered succesfully");
                return (true, null, "User registered succesfully", confirmationToken, user.Id);
            }
        }

        public async Task<(bool isSucceded, IEnumerable<string>? errors, string message)>
            ConfirmEmailAsync(ConfirmRequest confirmRequest, string token)
        {
            using (LogContext.PushProperty("Operation", nameof(ConfirmEmailAsync)))
            {
                if (confirmRequest == null || token == null)
                {
                    _logger.LogError("Failed to confirm user credentials");
                    return (false, new[] { "Invalid confirmation data" }, "Bad Request");
                }

                var existingUserByEmail = await _unitOfWork
                    .UserRepository.GetByEmailAsync(confirmRequest.Email);

                if (existingUserByEmail == null)
                {
                    _logger.LogError($"Couldn`t find user with email {existingUserByEmail.Email}");
                    return (false, new[] { "User with such email doesn`t exists" }, "Bad request");
                }
                if (existingUserByEmail.ConfirmationToken != token)
                {
                    _logger.LogError("Token expired");
                    return (false, new[] { "Invalid or expired reset token" }, "Unauthorized");
                }

                try
                {
                    _logger.LogInformation("Attemp to confirm user credentials");
                    existingUserByEmail.ConfirmationToken = null;
                    existingUserByEmail.IsConfirmed = true;

                    _unitOfWork.UserRepository.Update(existingUserByEmail);
                    await _unitOfWork.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Couldn`t verify email, {ex.Message}");
                    return (false, new[] { $"Couldn`t verify email, {ex.Message}" }, "Error");
                }

                _logger.LogInformation("Email confirmed succesfully");
                return (true, null, "Email confirmed succesfully");
            }
        }
    }
}
