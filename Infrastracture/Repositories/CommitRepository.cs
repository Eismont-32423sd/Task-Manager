using Domain.Abstractions;
using Domain.Entities.DbEntities;
using Infrastracture.Context;

namespace Infrastracture.Repositories
{
    public class CommitRepository : Repository<Commit>, ICommitRepository
    {
        public CommitRepository(ApplicationContext context) : base(context)
        {
            
        }
    }
}
