using Application.Services.DTOs.PorjectDTOs;
using Domain.Abstractions;
using Domain.Entities;

namespace Application.Features.Admin
{
    public class AdminUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        public AdminUserService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<(bool isSucceded, IEnumerable<string>? errors,
            string message, List<User>? users)> GetAllUsersAsync()
        {
            List<User> users = (List<User>)await _unitOfWork.UserRepository.GetAllAsync();

            if (users == null)
            {
                return (false, new[] { "There are no availvable users" }, "Conflict", null);
            }

            return (true, null, "Retrieved users", users);
        }

        public async Task<(bool isSucceded, IEnumerable<string>? errors, string message)>
            AssignOnProjectAsync(AssignRequest request)
        {
            if (request == null)
            {
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

            return (true, new[] { "User assigned to project succesfully" }, "Success");
        }

        public async Task<(bool isSucceded, IEnumerable<string>? errors, string message)>
            AssignRoleAsync(AssignRoleRequest request)
        {
            if (request == null)
            {
                return (false, new[] { "Invalid data" }, "Error");
            }

            var user = await _unitOfWork
                .UserRepository.GetByUserNameAsync(request.UserName);

            if(user == null)
            {
                return (false, new[] 
                { $"Couldn`t find user with username:{request.UserName}" }, "Conflict");
            }

            user.Role = request.Role;
            _unitOfWork.UserRepository.Update(user);
            await _unitOfWork.SaveChangesAsync();

            return (true, null, "Role assign to user succesfully");
        }
    }
}
