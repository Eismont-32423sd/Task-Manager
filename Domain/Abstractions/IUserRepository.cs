using Domain.Entities.DbEntities;

namespace Domain.Abstractions
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User> GetByEmailAsync(string email);
        Task<User> GetByUserNameAsync(string userName);
    }
}
