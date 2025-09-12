using Application.Services.DTOs.PorjectDTOs;
using Domain.Abstractions;
using Domain.Entities;

namespace Application.Features.User
{
    public class UserProjectService
    {
        private readonly IUnitOfWork _unitOfWork;
        public UserProjectService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<(bool isSucceded, IEnumerable<string>? errors, string message)>
            AddCommitAsync(CommitRequest request)
        {
            if (request == null)
            {
                return (false, new[] { "Please provide valid data" }, "Error");
            }

            var user = await _unitOfWork.UserRepository.GetByUserNameAsync(request.UserName);
            var stageAssignment = await _unitOfWork.StageAssignmentRepository.GetByStageTitleAsync(request.StageTitle);

            if (stageAssignment == null)
            {
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

            return (true, null, "Commit added successfully.");
        }
    }
}
