using Application.Services.DTOs.AuthenticationDTOS;
using Application.Services.Interfaces;
using Domain.Abstractions;
using Domain.Entities;
using FluentEmail.Core;

namespace Application.Features.Authentication
{
    public class AuthenticationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtGenerator _jwtGenerator;
        private readonly IFluentEmail _emailFactory;

        public AuthenticationService(IUnitOfWork unitOfWork,
                              IPasswordHasher passwordHasher,
                              IJwtGenerator jwtGenerator,
                              IFluentEmail emailFactory)
        {
            _unitOfWork = unitOfWork;
            _passwordHasher = passwordHasher;
            _jwtGenerator = jwtGenerator;
            _emailFactory = emailFactory;
        }

        public async Task<(bool isSucceded, IEnumerable<string>? errors,
            string message, string? ConfiramationToken, Guid? UserId)>
            RegisterAsync(RegisterRequest registerRequest, string confirmationLinkBaseUrl)
        {
            var existingUserbyEmail = await _unitOfWork
                .UserRepository.GetByEmailAsync(registerRequest.Email!);
            var existingUserByUserName = await _unitOfWork
                .UserRepository.GetByUserNameAsync(registerRequest.UserName!);

            if (existingUserbyEmail != null)
            {
                return (false, new[] { "User with such email already exists" }, "Conflict", null, null);
            }

            if (existingUserByUserName != null)
            {
                return (false, new[] { "User with such user name already exists" }, "Conflict", null, null);
            }

            var user = new User
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
                await _emailFactory.To(user.Email)
                    .Subject("Email verification for Task Manager")
                    .Body($"To verify you email <a href='{confirmationLink}'>click here</a>", isHtml: true)
                    .SendAsync();
            }
            catch (Exception ex)
            {
                return (false, new[] { "Failed to send confirmation email." },
                    $"User registered, but failed to send confirmation email. {ex.Message}", null, null);
            }

            return (true, null, "User registered succesfully", confirmationToken, user.Id);
        }

        public async Task<(bool isSucceded, IEnumerable<string>? errors, string message)>
            ConfirmEmailAsync(ConfirmRequest confirmRequest, string token)
        {
            if (confirmRequest == null || token == null)
            {
                return (false, new[] { "Invalid confirmation data" }, "Bad Request");
            }

            var existingUserByEmail = await _unitOfWork
                .UserRepository.GetByEmailAsync(confirmRequest.Email);

            if (existingUserByEmail == null)
            {
                return (false, new[] { "User with such email doesn`t exists" }, "Bad request");
            }
            if (existingUserByEmail.ConfirmationToken != token)
            {
                return (false, new[] { "Invalid or expired reset token" }, "Unauthorized");
            }


            try
            {
                existingUserByEmail.ConfirmationToken = null;
                existingUserByEmail.IsConfirmed = true;

                _unitOfWork.UserRepository.Update(existingUserByEmail);
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return (false, new[] { $"Couldn`t verify email, {ex.Message}" }, "Error");
            }

            return (true, null, "Email confirmed succesfully");
        }

        //public async Task<(bool isSucceded, IEnumerable<string> errors, string message, string? token)>
        //    LoginAsync(LoginRequest loginRequest)
        //{
        //    var user = await _unitOfWork.UserRepository
        //        .GetByUserNameAsync(loginRequest.UserName!);

        //    if (user == null)
        //    {
        //        return (false, new[] { "User with such user name doesn`t exist" }, "Conflict", null);
        //    }

        //    if (!_passwordHasher.Verify(loginRequest.Password!, user.PasswordHash!))
        //    {
        //        return (false, new[] { "Password doesn`t match" }, "Conflict", null);
        //    }
        //    if (!user.IsConfirmed)
        //    {
        //        return (false, new[] { "You did`nt confirm your data, please check your email box" }, "Conflict", null);
        //    }

        //    var verificationToken = _jwtGenerator.CreateJwtToken(user);

        //    return (true, null, "Logged in succesfully", verificationToken);
        //}

        //public async Task<(bool isScudded, IEnumerable<string>? errors, string message)>
        //    ResetAsync(ResetRequest resetRequest, string resetLinkBaseUrl)
        //{
        //    var user = await _unitOfWork
        //        .UserRepository.GetByEmailAsync(resetRequest.Email);

        //    if (user == null)
        //    {
        //        return (false, new[] { "Could`nt find and email, try again" }, "Conflict");
        //    }

        //    var resetToken = _jwtGenerator.CreateJwtToken(user);
        //    user.ConfirmationToken = resetToken;
        //    user.IsConfirmed = false;
        //    user.PasswordHash = null;
        //    await _unitOfWork.SaveChangesAsync();

        //    var confirmationLink = $"{resetLinkBaseUrl}?userId={user.Id}&token={Uri.EscapeDataString(resetToken)}";

        //    try
        //    {
        //        await _emailFactory.To(user.Email)
        //            .Subject("Email verification for Task Manager")
        //            .Body($"To verify you email <a href='{confirmationLink}'>click here</a>", isHtml: true)
        //            .SendAsync();
        //    }
        //    catch (Exception ex)
        //    {
        //        return (false, new[] { $"Could`nt send an email, {ex.Message}" }, "Error");
        //    }

        //    return (true, null, "Reset email was sent to your mailbox");
        //}

        //public async Task<(bool isSuccede, IEnumerable<string>? errors, string message)>
        //    UpdateCredentialsAsync(UpdateRequest updateRequest, string token)
        //{
        //    if (updateRequest == null)
        //    {
        //        return (false, new[] { "Error updating credentials" }, "Error");
        //    }

        //    var user = await _unitOfWork
        //        .UserRepository.GetByEmailAsync(updateRequest.Email);
        //    if (user == null)
        //    {
        //        return (false, new[] { "User with such email doesn`t exist" }, "Conflict");
        //    }
        //    if (user.ConfirmationToken != token)
        //    {
        //        return (false, new[] { "Invalid or expired reset token" }, "Unauthorized");
        //    }

        //    try
        //    {
        //        user.UserName = updateRequest.UserName;
        //        user.PasswordHash = _passwordHasher.Hash(updateRequest.Password);
        //        user.ConfirmationToken = null;
        //        user.IsConfirmed = true;
        //        _unitOfWork.UserRepository.Update(user);
        //        await _unitOfWork.SaveChangesAsync();
        //    }
        //    catch (Exception ex)
        //    {
        //        return (false,
        //            new[] { $"Error occured while updating credentials, please try again; {ex.Message}" }, "Error");
        //    }
        //    return (true, null, "User credentials updated succesfully");
        //}
    }
}
