using Domain.Entities.DbEntities;

namespace Domain.Abstractions
{
    public interface IProjectRepository : IRepository<Project>
    {
        Task<Project> GetByTitleAsync(string title);
        Task<Project> GetByEndDateAsync(DateOnly endDate);
        Task<Project> GetByStartDateAsync(DateOnly startDate);
        Task<IEnumerable<Project>> GetAllWithParticipantsAsync();
    }
}
