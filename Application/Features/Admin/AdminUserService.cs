using Application.Services.DTOs.Generic;
using Application.Services.DTOs.PorjectDTOs;
using Domain.Abstractions;
using Microsoft.Extensions.Logging;
using Serilog.Context;

namespace Application.Features.Admin
{
    public class AdminUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AdminUserService> _logger;
        public AdminUserService(IUnitOfWork unitOfWork,
            ILogger<AdminUserService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<ServiceResult<List<Domain.Entities.DbEntities.User>>> GetAllUsersAsync()
        {
            using (LogContext.PushProperty("Operation", nameof(GetAllUsersAsync)))
            {
                List<Domain.Entities.DbEntities.User> users = 
                    (List<Domain.Entities.DbEntities.User>)await _unitOfWork.UserRepository.GetAllAsync();

                if (users == null)
                {
                    _logger.LogError("Couldn`t find users");
                    return new ServiceResult<List<Domain.Entities.DbEntities.User>>
                    {
                        IsSucceded = false,
                        Errors = new[] { "There are no availvable users" },
                        Message = "Conflict",
                        Data = null
                    };
                }

                _logger.LogInformation("Users retrieved succesfully");
                return new ServiceResult<List<Domain.Entities.DbEntities.User>>
                {
                    IsSucceded = true,
                    Errors = null,
                    Message = "Users retrieved succesfully",
                    Data = users
                };
            }
        }

        public async Task<ServiceResult<object>>
            AssignOnProjectAsync(AssignRequest request)
        {
            using (LogContext.PushProperty("Operation", nameof(AssignOnProjectAsync)))
            {
                if (request == null)
                {
                    _logger.LogError("Failed to assigned user to project");
                    return new ServiceResult<object>
                    {
                        IsSucceded = false,
                        Errors = new[] { "Invalid data" },
                        Message = "Error",
                        Data = null
                    };
                }

                var project = await _unitOfWork
                    .ProjectRepository.GetByTitleAsync(request.Titile);

                foreach (var userName in request.UserNames)
                {
                    var user = await _unitOfWork
                        .UserRepository.GetByUserNameAsync(userName);

                    project.Participants.Add(user);

                }

                _unitOfWork.ProjectRepository.Update(project);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("User assigned to project succesfully");
                return new ServiceResult<object>
                {
                    IsSucceded = true,
                    Errors = null,
                    Message = "Success",
                    Data = null
                };
            }
        }

        public async Task<ServiceResult<object>>
            AssignRoleAsync(AssignRoleRequest request)
        {
            using (LogContext.PushProperty("Operation", nameof(AssignRoleAsync)))
            {
                if (request == null)
                {
                    _logger.LogError("Failed to assign role to user");
                    return new ServiceResult<object>
                    {
                        IsSucceded = false,
                        Errors = new[] { "Invalid data" },
                        Message = "Error",
                        Data = null
                    };
                }

                var user = await _unitOfWork
                    .UserRepository.GetByUserNameAsync(request.UserName);

                if (user == null)
                {
                    _logger.LogError($"Couldn`t find user with username:{request.UserName}");
                    return new ServiceResult<object>
                    {
                        IsSucceded = false,
                        Errors = new[] { $"Couldn`t find user with username:{request.UserName}" },
                        Message = "Conflict",
                        Data = null
                    };
                }

                user.Role = request.Role;
                _unitOfWork.UserRepository.Update(user);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Role assigned to user succesfully");
                return new ServiceResult<object>
                {
                    IsSucceded = true,
                    Errors = null,
                    Message = "Role assigned to user succesfully",
                    Data = null
                };
            }
        }

        public async Task<ServiceResult<object>>
            DeleteUserAsync(string userName)
        {
            using (LogContext.PushProperty("Operation", nameof(DeleteUserAsync)))
            {
                if (userName == null)
                {
                    _logger.LogError($"Couldn`t find user with username:{userName}");
                    return new ServiceResult<object>
                    {
                        IsSucceded = false,
                        Errors = new[] { $"Couldn`t find user with username:{userName}" },
                        Message = "Not Found",
                        Data = null
                    };
                }

                var user = await _unitOfWork.
                    UserRepository.GetByUserNameAsync(userName);

                _unitOfWork.UserRepository.Delete(user);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("User removed succesfully");
                return new ServiceResult<object>
                {
                    IsSucceded = true,
                    Errors = null,
                    Message = "User removed succesfully",
                    Data = null
                };
            }
        }
    }
}
