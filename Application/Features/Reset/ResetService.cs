using Application.Services.DTOs.AuthenticationDTOS;
using Application.Services.Interfaces;
using Domain.Abstractions;
using FluentEmail.Core;

namespace Application.Features.Reset
{
    public class ResetService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IJwtGenerator _jwtGenerator;
        private readonly IFluentEmail _emailFactory;
        private readonly IPasswordHasher _passwordHasher;

        public ResetService(IUnitOfWork unitOfWork,
            IJwtGenerator jwtGenerator,
            IFluentEmail emailFactory,
            IPasswordHasher passwordHasher)
        {
            _unitOfWork = unitOfWork;
            _jwtGenerator = jwtGenerator;
            _emailFactory = emailFactory;
            _passwordHasher = passwordHasher;
        }

        public async Task<(bool isScudded, IEnumerable<string>? errors, string message)>
            ResetAsync(ResetRequest resetRequest, string resetLinkBaseUrl)
        {
            var user = await _unitOfWork
                .UserRepository.GetByEmailAsync(resetRequest.Email);

            if (user == null)
            {
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
                await _emailFactory.To(user.Email)
                    .Subject("Email verification for Task Manager")
                    .Body($"To verify you email <a href='{confirmationLink}'>click here</a>", isHtml: true)
                    .SendAsync();
            }
            catch (Exception ex)
            {
                return (false, new[] { $"Could`nt send an email, {ex.Message}" }, "Error");
            }

            return (true, null, "Reset email was sent to your mailbox");
        }

        public async Task<(bool isSuccede, IEnumerable<string>? errors, string message)>
            UpdateCredentialsAsync(UpdateRequest updateRequest, string token)
        {
            if (updateRequest == null)
            {
                return (false, new[] { "Error updating credentials" }, "Error");
            }

            var user = await _unitOfWork
                .UserRepository.GetByEmailAsync(updateRequest.Email);
            if (user == null)
            {
                return (false, new[] { "User with such email doesn`t exist" }, "Conflict");
            }
            if (user.ConfirmationToken != token)
            {
                return (false, new[] { "Invalid or expired reset token" }, "Unauthorized");
            }

            try
            {
                user.UserName = updateRequest.UserName;
                user.PasswordHash = _passwordHasher.Hash(updateRequest.Password);
                user.ConfirmationToken = null;
                user.IsConfirmed = true;
                _unitOfWork.UserRepository.Update(user);
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return (false,
                    new[] { $"Error occured while updating credentials, please try again; {ex.Message}" }, "Error");
            }

            return (true, null, "User credentials updated succesfully");
        }
    }
}
