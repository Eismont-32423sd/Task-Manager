using Application.Services.DTOs.AuthenticationDTOS;
using Application.Services.Interfaces;
using Domain.Abstractions;
using FluentEmail.Core;
using Microsoft.Extensions.Logging;
using Serilog.Context;

namespace Application.Features.Reset
{
    public class ResetService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IJwtGenerator _jwtGenerator;
        private readonly IFluentEmail _emailFactory;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ILogger<ResetService> _logger;
        public ResetService(IUnitOfWork unitOfWork,
            IJwtGenerator jwtGenerator,
            IFluentEmail emailFactory,
            IPasswordHasher passwordHasher,
            ILogger<ResetService> logger)
        {
            _unitOfWork = unitOfWork;
            _jwtGenerator = jwtGenerator;
            _emailFactory = emailFactory;
            _passwordHasher = passwordHasher;
            _logger = logger;
        }

        public async Task<(bool isScudded, IEnumerable<string>? errors, string message)>
            ResetAsync(ResetRequest resetRequest, string resetLinkBaseUrl)
        {
            using (LogContext.PushProperty("Operation", nameof(ResetAsync)))
            {
                var user = await _unitOfWork
                    .UserRepository.GetByEmailAsync(resetRequest.Email);

                if (user == null)
                {
                    _logger.LogInformation($"Failed to find email, {user.Email}");
                    return (false, new[] { "Could`nt find and email, try again" }, "Conflict");
                }

                var resetToken = _jwtGenerator.CreateJwtToken(user);
                user.ConfirmationToken = resetToken;
                user.IsConfirmed = false;
                user.PasswordHash = null;
                await _unitOfWork.SaveChangesAsync();

                var confirmationLink = $"{resetLinkBaseUrl}?userId={user.Id}&token={Uri.EscapeDataString(resetToken)}";

                try
                {
                    _logger.LogInformation("Attemp to send an email");
                    await _emailFactory.To(user.Email)
                        .Subject("Email verification for Task Manager")
                        .Body($"To verify you email <a href='{confirmationLink}'>click here</a>", isHtml: true)
                        .SendAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Could`nt send an email, {ex.Message}");
                    return (false, new[] { $"Could`nt send an email, {ex.Message}" }, "Error");
                }

                _logger.LogInformation("Reset email has been sent");
                return (true, null, "Reset email was sent to your mailbox");
            }
        }

        public async Task<(bool isSuccede, IEnumerable<string>? errors, string message)>
            UpdateCredentialsAsync(UpdateRequest updateRequest, string token)
        {
            using (LogContext.PushProperty("Operation", nameof(UpdateCredentialsAsync)))
            {
                if (updateRequest == null)
                {
                    _logger.LogError($"Invalid update request, {updateRequest}");
                    return (false, new[] { "Error updating credentials" }, "Error");
                }

                var user = await _unitOfWork
                    .UserRepository.GetByEmailAsync(updateRequest.Email);
                if (user == null)
                {
                    _logger.LogError($"Couldn`t find user with email:{user.Email}");
                    return (false, new[] { "User with such email doesn`t exist" }, "Conflict");
                }
                if (user.ConfirmationToken != token)
                {
                    _logger.LogError("Expired token");
                    return (false, new[] { "Invalid or expired reset token" }, "Unauthorized");
                }

                try
                {
                    _logger.LogInformation("Attemp to update credantials");
                    user.UserName = updateRequest.UserName;
                    user.PasswordHash = _passwordHasher.Hash(updateRequest.Password);
                    user.ConfirmationToken = null;
                    user.IsConfirmed = true;
                    _unitOfWork.UserRepository.Update(user);
                    await _unitOfWork.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError("Failed to update credentails");
                    return (false,
                        new[] { $"Error occured while updating credentials, please try again; {ex.Message}" }, "Error");
                }

                _logger.LogInformation("User credentials updated succesfully");
                return (true, null, "User credentials updated succesfully");
            }
        }
    }
}
