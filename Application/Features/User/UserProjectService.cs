using Application.Services.DTOs.Generic;
using Application.Services.DTOs.PorjectDTOs;
using Domain.Abstractions;
using Domain.Entities.DbEntities;
using Microsoft.Extensions.Logging;
using Serilog.Context;
using Serilog.Core;

namespace Application.Features.User
{
    public class UserProjectService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UserProjectService> _logger;
        public UserProjectService(IUnitOfWork unitOfWork,
            ILogger<UserProjectService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<ServiceResult<object>>
            AddCommitAsync(CommitRequest request)
        {
            using (LogContext.PushProperty("Operation", nameof(AddCommitAsync)))
            {
                if (request == null)
                {
                    _logger.LogError($"Provided invalid commit request, {request}");
                    return new ServiceResult<object>
                    {
                        IsSucceded = false,
                        Errors = new[] { "Please provide valid data" },
                        Message = "Error",
                        Data = null
                    };
                }

                var user = await _unitOfWork.UserRepository.GetByUserNameAsync(request.UserName);
                var stageAssignment = await _unitOfWork.StageAssignmentRepository.GetByStageTitleAsync(request.StageTitle);

                if (stageAssignment == null)
                {
                    _logger.LogError($"Error, user {user.UserName} is not assigned to the stage");
                    return new ServiceResult<object>
                    {
                        IsSucceded = false,
                        Errors = new[] { $"Error, user {user.UserName} is not assigned to the stage" },
                        Message = "Error",
                        Data = null
                    };
                }

                var newCommit = new Commit
                {
                    StageAssignmentStageId = stageAssignment.StageId,
                    StageAssignmentUserId = stageAssignment.UserId,
                    Message = request.CommitMessage,
                    CommitDate = DateOnly.FromDateTime(DateTime.UtcNow)
                };

                await _unitOfWork.CommitRepository.AddAsync(newCommit);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Commit created successfully");
                return new ServiceResult<object>
                {
                    IsSucceded = true,
                    Errors = null,
                    Message = "Commit created successfully",
                    Data = null
                };
            }
        }

        public async Task<ServiceResult<List<Commit>>>
            GetAllCommitsAsync(Guid userId)
        {
            using (LogContext.PushProperty("Operation", nameof(GetAllCommitsAsync)))
            {
                var commits = await _unitOfWork.CommitRepository.GetUserCommitsAsync(userId);

                if (!commits.Any())
                {
                    return new ServiceResult<List<Commit>>
                    {
                        IsSucceded = false,
                        Errors = new[] { "No commits found for this user" },
                        Message = "Error",
                        Data = null
                    };
                }

                return new ServiceResult<List<Commit>>
                {
                    IsSucceded = true,
                    Errors = null,
                    Message = "Commits retrieved successfully",
                    Data = commits
                };
            }
        }
    }
}
