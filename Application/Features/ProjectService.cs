using Application.Services.DTOs.AuthenticationDTOS;
using Application.Services.DTOs.PorjectDTOs;
using Domain.Abstractions;
using Domain.Entities;

namespace Application.Features
{
    public class ProjectService
    {
        private readonly IUnitOfWork _unitOfWork;
        public ProjectService(IUnitOfWork unitOfWork)
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

        public async Task<(bool isSucceded, IEnumerable<string>? errors,
                string message, List<ProjectDto>? projects)> GetAllProjectsAsync()
        {
            var projects = await _unitOfWork.ProjectRepository.GetAllWithParticipantsAsync();

            if (projects == null || !projects.Any())
            {
                return (false, new[] { "There are no available projects" }, "Conflict", null);
            }

            var projectDtos = projects.Select(p => new ProjectDto
            {
                Id = p.Id,
                Title = p.Title,
                Description = p.Description,
                Participants = p.Participants.Select(u => new UserDto
                {
                    Id = u.Id,
                    UserName = u.UserName
                }).ToList()
            }).ToList();

            return (true, null, "Retrieved projects", projectDtos);
        }

        public async Task<(bool isSucceded, IEnumerable<string>? errors, string message)>
            CreateProjectAsync(CreateProjectRequest request)
        {
            if (request == null)
            {
                return (false, new[] { "Error occured while creating new project" }, "Error");
            }

            var project = new Project
            {
                Id = Guid.NewGuid(),
                Title = request.Title,
                Description = request.Description,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                ProjectType = request.ProjectType,
                Status = request.Status,
                OwnerId = request.OwnerId
            };

            await _unitOfWork.ProjectRepository.AddAsync(project);
            await _unitOfWork.SaveChangesAsync();

            return (true, null, "Project added succesfully");
        }

        public async Task<(bool isSucceded, IEnumerable<string>? errors, string message, Project? project)>
            GetProjectByIdAsync(Guid projectId)
        {
            if (projectId == Guid.Empty)
            {
                return (false, new[] { "Provide valid project id" },
                    "Error", null);
            }

            var project = await _unitOfWork.ProjectRepository
                .GetByIdAsync(projectId);
            if (project == null)
            {
                return (false, new[] { $"Unable to find project with id = " +
                    $"{projectId}" }, "Error", null);
            }

            return (true, null, "Project retrieved succesfully", project);
        }

        public async Task<(bool isSucceded, IEnumerable<string>? errors, string message)>
            UpdateProjectAsync(string projectTitle, UpdateProjectRequest request)
        {
            if (projectTitle == null)
            {
                return (false, new[] { "Please provide project title" }, "Not Found");
            }

            if (request == null)
            {
                return (false, new[] { "Invalid update data" }, "Conflict");
            }

            var project = await _unitOfWork
                .ProjectRepository.GetByTitleAsync(projectTitle);
            if (project == null)
            {
                return (false, new[] { $"Couldn`t find porject with title {projectTitle}" }, "Not Found");
            }

            project.StartDate = request.StartDate;
            project.EndDate = request.EndDate;
            project.Title = request.Title;
            project.Description = request.Description;
            project.Status = request.Status;
            project.ProjectType = request.ProjectType;

            _unitOfWork.ProjectRepository.Update(project);
            await _unitOfWork.SaveChangesAsync();

            return (true, null, "Project data changed succesfully");
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
    }
}
