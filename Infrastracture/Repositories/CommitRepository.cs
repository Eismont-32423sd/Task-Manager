using Domain.Abstractions;
using Domain.Entities.DbEntities;
using Infrastracture.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastracture.Repositories
{
    public class CommitRepository : Repository<Commit>, ICommitRepository
    {
        public CommitRepository(ApplicationContext context) : base(context)
        {

        }

        public async Task<List<Commit>> GetUserCommitsAsync(Guid userId)
        {
            return await _dbSet.Where(c => c.StageAssignmentUserId == userId)
                .ToListAsync();
        }
    }
}
