using Application.Services.DTOs.AuthenticationDTOS;
using Application.Services.DTOs.Generic;
using Application.Services.DTOs.PorjectDTOs;
using Domain.Abstractions;
using Domain.Entities.DbEntities;
using Microsoft.Extensions.Logging;
using Serilog.Context;

namespace Application.Features.Admin
{
    public class AdminProjectService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AdminProjectService> _logger;
        public AdminProjectService(IUnitOfWork unitOfWork,
            ILogger<AdminProjectService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<ServiceResult<List<ProjectDto>>> GetAllProjectsAsync()
        {
            using (LogContext.PushProperty("Operation", nameof(GetAllProjectsAsync)))
            {
                var projects = await _unitOfWork.ProjectRepository.GetAllWithParticipantsAsync();

                if (projects == null || !projects.Any())
                {
                    _logger.LogError("There are no available projects");
                    return new ServiceResult<List<ProjectDto>>
                    {
                        IsSucceded = false,
                        Message = "Conflict",
                        Errors = new[] { "There are no available projects" }
                    };
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

                _logger.LogInformation("Retrieved projects successfully");

                return new ServiceResult<List<ProjectDto>>
                {
                    IsSucceded = true,
                    Message = "Retrieved projects",
                    Data = projectDtos
                };
            }
        }


        public async Task<ServiceResult<object>> CreateProjectAsync(CreateProjectRequest request)
        {
            using (LogContext.PushProperty("Operation", nameof(CreateProjectAsync)))
            {
                if (request == null)
                {
                    _logger.LogError("Error occured while creating new project");
                    return new ServiceResult<object>
                    {
                        IsSucceded = false,
                        Message = "Error",
                        Errors = new[] { "Error occured while creating new project" }
                    };
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
                };

                await _unitOfWork.ProjectRepository.AddAsync(project);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Project added succesfully");
                return new ServiceResult<object>
                {
                    IsSucceded = true,
                    Message = "Project added succesfully"
                };
            }
        }

        public async Task<ServiceResult<Project>>
            GetProjectByIdAsync(Guid porjectId)
        {
            using (LogContext.PushProperty("Operation", nameof(GetProjectByIdAsync)))
            {
                if (porjectId == Guid.Empty)
                {
                    _logger.LogError("Provide valid project id");
                    return new ServiceResult<Project>
                    {
                        IsSucceded = false,
                        Message = "Provide valid project id",
                        Data = null
                    };
                }

                var project = await _unitOfWork.ProjectRepository
                    .GetByIdAsync(porjectId);
                if (project == null)
                {
                    _logger.LogError($"Unable to find project with id = {porjectId}");;
                    return new ServiceResult<Project>
                    {
                        IsSucceded = false,
                        Message = $"Unable to find project with id = {porjectId}",
                        Data = null
                    };
                }

                _logger.LogInformation("Project retrieved succesfully");
                return new ServiceResult<Project>
                {
                    IsSucceded = true,
                    Message = "Project retrieved succesfully",
                    Data = project
                };
            }
        }

        public async Task<ServiceResult<object>>
            UpdateProjectAsync(string projectTitle, UpdateProjectRequest request)
        {
            using (LogContext.PushProperty("Operation", nameof(UpdateProjectAsync)))
            {

                if (projectTitle == null)
                {
                    _logger.LogError("Please provide project title");
                    return new ServiceResult<object>
                    {
                        IsSucceded = false,
                        Errors = new[] { "Please provide project title" },
                        Message = "Not Found",
                        Data = null
                    };
                }

                if (request == null)
                {
                    _logger.LogError("Invalid update data");
                    return new ServiceResult<object>
                    {
                        IsSucceded = false,
                        Errors = new[] { "Invalid update data" },
                        Message = "Conflict",
                        Data = null
                    };
                }

                var project = await _unitOfWork
                    .ProjectRepository.GetByTitleAsync(projectTitle);
                if (project == null)
                {
                    _logger.LogError($"Couldn`t find porject with title {projectTitle}");
                    return new ServiceResult<object>
                    {
                        IsSucceded = false,
                        Errors = new[] { $"Couldn`t find porject with title {projectTitle}" },
                        Message = "Not Found",
                        Data = null
                    };
                }

                project.StartDate = request.StartDate;
                project.EndDate = request.EndDate;
                project.Title = request.Title;
                project.Description = request.Description;
                project.Status = request.Status;
                project.ProjectType = request.ProjectType;

                _unitOfWork.ProjectRepository.Update(project);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Project data changed succesfully");
                return new ServiceResult<object>
                {
                    IsSucceded = true,
                    Errors = null,
                    Message = "Project data changed succesfully",
                    Data = null
                };
            }
        }

        public async Task<ServiceResult<object>>
            DeleteProjectAsync(string title)
        {
            using (LogContext.PushProperty("Operation", nameof(DeleteProjectAsync)))
            {
                if (title == null)
                {
                    _logger.LogError($"Could`nt find project with title:{title}");
                    return new ServiceResult<object>
                    {
                        IsSucceded = false,
                        Errors = new[] { $"Could`nt find project with title:{title}" },
                        Message = "Not Found",
                        Data = null
                    };

                }

                var project = await _unitOfWork
                    .ProjectRepository.GetByTitleAsync(title);

                if (project == null)
                {
                    _logger.LogError($"Could`nt find project with title:{title}");
                    return new ServiceResult<object>
                    {
                        IsSucceded = false,
                        Errors = new[] { $"Could`nt find project with title:{title}" },
                        Message = "Not Found",
                        Data = null
                    };
                }

                _unitOfWork.ProjectRepository.Delete(project);
                await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation($"Delete project: {title}");
                return new ServiceResult<object>
                {
                    IsSucceded = true,
                    Errors = null,
                    Message = $"Project titled {title} deleted succesfully",
                    Data = null
                };
            }
        }

        public async Task<ServiceResult<object>>
            AddStagesToProjectAsync(AddStageRequest request)
        {
            using (LogContext.PushProperty("Operation", nameof(AddStagesToProjectAsync)))
            {
                if (request == null)
                {
                    _logger.LogError("Provided invalid data");;
                    return new ServiceResult<object>
                    {
                        IsSucceded = false,
                        Errors = new[] { "Provide valid data" },
                        Message = "Bad Request",
                        Data = null
                    };
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

                _logger.LogInformation("Stage added succesfully");
                return new ServiceResult<object>
                {
                    IsSucceded = true,
                    Errors = null,
                    Message = "Stage added succesfully",
                    Data = null
                };
            }
        }
    }
}
