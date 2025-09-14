using Application.Services.DTOs.Generic;
using Application.Services.Interfaces;
using Domain.Abstractions;
using Domain.Entities.Enums;
using Microsoft.Extensions.Logging;
using Serilog.Context;
using static System.Runtime.InteropServices.JavaScript.JSType;
namespace Application.Features.Admin
{
    public class AddAdminService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ILogger<AddAdminService> _logger;

        public AddAdminService(IUnitOfWork unitOfWork,
            IPasswordHasher passwordHasher,
            ILogger<AddAdminService> logger)
        {
            _unitOfWork = unitOfWork;
            _passwordHasher = passwordHasher;
            _logger = logger;
        }

        public async Task<ServiceResult<object>> AddAdminAsync()
        {
            using (LogContext.PushProperty("Operation", nameof(AddAdminAsync)))
            {
                var existingAdmin = await _unitOfWork
                    .UserRepository.GetByUserNameAsync("Admin");
                if (existingAdmin != null)
                {
                    return new ServiceResult<object>
                    {
                        IsSucceded = false,
                        Errors =new[] { "Admin has already been created" },
                        Message = "Conflict",
                        Data = null
                    };
                }

                _logger.LogInformation("Attempting to create admin user");
                string password = "strong_admin_pass_123";
                var adminUser = new Domain.Entities.DbEntities.User
                {
                    Id = Guid.NewGuid(),
                    UserName = "Admin",
                    Email = "Admin@gmail.com",
                    PasswordHash = _passwordHasher.Hash(password),
                    IsConfirmed = true,
                    Role = Role.TeamLead
                };
                await _unitOfWork.UserRepository.AddAsync(adminUser);
                await _unitOfWork.SaveChangesAsync();

                return new ServiceResult<object>
                {
                    IsSucceded = true,
                    Errors = null,
                    Message = "Admin with username " +
                    $"{adminUser.UserName} and password {password} created succesfully",
                    Data = null
                };
            }
        }
    }
}
