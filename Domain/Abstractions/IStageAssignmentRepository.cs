using Domain.Entities.DbEntities;

namespace Domain.Abstractions
{
    public interface IStageAssignmentRepository : IRepository<StageAssignment>
    {
        Task AddRangeAsync(List<StageAssignment> stageAssignments);
        Task<StageAssignment> GetByStageTitleAsync(string stageTitle);
    }
}
