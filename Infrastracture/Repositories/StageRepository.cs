using Domain.Abstractions;
using Domain.Entities.DbEntities;
using Infrastracture.Context;

namespace Infrastracture.Repositories
{
    public class StageRepository : Repository<Stage>, IStageRepository
    {
        public StageRepository(ApplicationContext context) : base(context)
        {
        }
    }
}
