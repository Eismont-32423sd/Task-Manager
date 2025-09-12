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

        public async Task<(bool isSucceded, IEnumerable<string>? errors,
            string message, List<Domain.Entities.User>? users)> GetAllUsersAsync()
        {
            using (LogContext.PushProperty("Operation", nameof(GetAllUsersAsync)))
            {
                List<Domain.Entities.User> users = (List<Domain.Entities.User>)await _unitOfWork.UserRepository.GetAllAsync();

                if (users == null)
                {
                    _logger.LogError("Couldn`t find users");
                    return (false, new[] { "There are no availvable users" }, "Conflict", null);
                }

                _logger.LogInformation("Users retrieved succesfully");
                return (true, null, "Retrieved users", users);
            }
        }

        public async Task<(bool isSucceded, IEnumerable<string>? errors, string message)>
            AssignOnProjectAsync(AssignRequest request)
        {
            using (LogContext.PushProperty("Operation", nameof(AssignOnProjectAsync)))
            {
                if (request == null)
                {
                    _logger.LogError("Failed to assigned user to project");
                    return (false, new[] { "Invalid data" }, "Error");
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
                return (true, new[] { "User assigned to project succesfully" }, "Success");
            }
        }

        public async Task<(bool isSucceded, IEnumerable<string>? errors, string message)>
            AssignRoleAsync(AssignRoleRequest request)
        {
            using (LogContext.PushProperty("Operation", nameof(AssignRoleAsync)))
            {
                if (request == null)
                {
                    _logger.LogError("Failed to assign role to user");
                    return (false, new[] { "Invalid data" }, "Error");
                }

                var user = await _unitOfWork
                    .UserRepository.GetByUserNameAsync(request.UserName);

                if (user == null)
                {
                    _logger.LogError($"Couldn`t find user with username:{request.UserName}");
                    return (false, new[]
                    { $"Couldn`t find user with username:{request.UserName}" }, "Conflict");
                }

                user.Role = request.Role;
                _unitOfWork.UserRepository.Update(user);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Role assigned to user succesfully");
                return (true, null, "Role assigned to user succesfully");
            }
        }

        public async Task<(bool isSucceded, IEnumerable<string>? errors, string message)>
            DeleteUserAsync(string userName)
        {
            using (LogContext.PushProperty("Operation", nameof(DeleteUserAsync)))
            {
                if (userName == null)
                {
                    _logger.LogError($"Couldn`t find user with username:{userName}");
                    return (false, new[] { $"Couldn`t find user with username:{userName}" }, "Not Found");
                }

                var user = await _unitOfWork.
                    UserRepository.GetByUserNameAsync(userName);

                _unitOfWork.UserRepository.Delete(user);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("User removed succesfully");
                return (true, null, "User removed succesfully");
            }
        }
    }
}
