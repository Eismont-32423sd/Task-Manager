using Application.Services.DTOs.PorjectDTOs;
using Domain.Abstractions;
using Domain.Entities;
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

        public async Task<(bool isSucceded, IEnumerable<string>? errors, string message)>
            AddCommitAsync(CommitRequest request)
        {
            using (LogContext.PushProperty("Operation", nameof(AddCommitAsync)))
            {
                if (request == null)
                {
                    _logger.LogError($"Provided invalid commit request, {request}");
                    return (false, new[] { "Please provide valid data" }, "Error");
                }

                var user = await _unitOfWork.UserRepository.GetByUserNameAsync(request.UserName);
                var stageAssignment = await _unitOfWork.StageAssignmentRepository.GetByStageTitleAsync(request.StageTitle);

                if (stageAssignment == null)
                {
                    _logger.LogError($"Error, user {user.UserName} is not assigned to the stage");
                    return (false, new[] { "User is not assigned to this stage." }, "Error");
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
                return (true, null, "Commit added successfully.");
            }
        }

        public async Task<(bool isSucceded, IEnumerable<string>? errors, string message, List<Commit>? commits)>
            GetAllCommitsAsync()
        {
            using (LogContext.PushProperty("Operation", nameof(GetAllCommitsAsync)))
            {
                List<Commit> commitments = (List<Commit>)await _unitOfWork.CommitRepository.GetAllAsync();
                if (commitments == null)
                {
                    _logger.LogInformation("Zero commitments");
                    return (false, new[] { "Couldn`t find any commits" }, "Errors", null);
                }
                _logger.LogInformation("Commits retrieved succesfully");
                return (true, null, "Commits retrieved succesfully", commitments);
            }
        }
    }
}
