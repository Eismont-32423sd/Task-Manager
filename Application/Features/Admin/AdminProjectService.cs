using Application.Services.DTOs.AuthenticationDTOS;
using Application.Services.DTOs.PorjectDTOs;
using Domain.Abstractions;
using Domain.Entities;

namespace Application.Features.Admin
{
    public class AdminProjectService
    {
        private readonly IUnitOfWork _unitOfWork;
        public AdminProjectService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
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
            DeleteProjectAsync(string title)
        {
            if (title == null)
            {
                return (false, new[]
                { $"Could`nt find project with title:{title}" }, "Not Found");
            }

            var project = await _unitOfWork
                .ProjectRepository.GetByTitleAsync(title);

            if (project == null)
            {
                return (false, new[]
                { $"Could`nt find project with title:{title}" }, "Not Found");
            }

            _unitOfWork.ProjectRepository.Delete(project);
            await _unitOfWork.SaveChangesAsync();

            return (true, null, $"Project titled {title} deleted succesfully");
        }

        public async Task<(bool isSucceded, IEnumerable<string>? errors, string message)>
            AddStagesToProjectAsync(AddStageRequest request)
        {
            if (request == null)
            {
                return (false, new[] { "Provide valid data" }, "Bad Request");
            }

            var project = await _unitOfWork
                .ProjectRepository.GetByIdAsync(request.ProjectId);

            var newStage = new Stage
            {
                Id = Guid.NewGuid(),
                Title = request.Title,
                ProjectId = request.ProjectId
            };
            await _unitOfWork.StageRepository.AddAsync(newStage);

            var stageAssignments = request.AssignedUserIds.Select(userId => new StageAssignment
            {
                StageId = newStage.Id,
                UserId = userId,
                StageTitle = request.Title,
                IsCompleted = false
            }).ToList();

            foreach (var userId in request.AssignedUserIds)
            {
                var user = await _unitOfWork.UserRepository.GetByIdAsync(userId);
                user.StageId = newStage.Id;
            }
            await _unitOfWork.StageAssignmentRepository.AddRangeAsync(stageAssignments);
            await _unitOfWork.SaveChangesAsync();

            return (true, null, "Stage added succesfully");
        }
    }
}
