using Domain.Entities.DbEntities;

namespace Domain.Abstractions
{
    public interface ICommitRepository : IRepository<Commit>
    {
        Task<List<Commit>> GetUserCommitsAsync(Guid userId);
    }
}
