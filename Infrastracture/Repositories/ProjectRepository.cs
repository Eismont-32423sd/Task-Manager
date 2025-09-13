using Domain.Abstractions;
using Domain.Entities.DbEntities;
using Infrastracture.Context;
using Microsoft.EntityFrameworkCore;


namespace Infrastracture.Repositories
{
    public class ProjectRepository : Repository<Project>, IProjectRepository
    {
        public ProjectRepository(ApplicationContext context) : base(context) { }

        public async Task<Project> GetByEndDateAsync(DateOnly endDate)
        {
            return await _dbSet
                .FirstOrDefaultAsync(x => x.EndDate == endDate);
        }

        public async Task<Project> GetByStartDateAsync(DateOnly startDate)
        {
            return await _dbSet
                .FirstOrDefaultAsync(x => x.StartDate == startDate);
        }

        public Task<Project> GetByTitleAsync(string title)
        {
            return _dbSet.Include(p => p.Participants)
                .FirstOrDefaultAsync(x => x.Title == title);
        }
        public async Task<IEnumerable<Project>> GetAllWithParticipantsAsync()
        {
            return await _dbSet
                .Include(p => p.Participants)
                .ToListAsync();
        }
    }
}
