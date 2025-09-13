using Domain.Abstractions;
using Domain.Entities.DbEntities;
using Infrastracture.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastracture.Repositories
{
    public class StageAssignmentRepository : Repository<StageAssignment>, IStageAssignmentRepository
    {
        public StageAssignmentRepository(ApplicationContext context) : base(context)
        {
            
        }

        public async Task AddRangeAsync(List<StageAssignment> stageAssignments)
        {
            await _dbSet.AddRangeAsync(stageAssignments);
        }
        public async Task<StageAssignment> GetByStageTitleAsync(string stageTitle)
        {
            return await _dbSet.FirstOrDefaultAsync(s => s.StageTitle == stageTitle);
        }
    }
}
