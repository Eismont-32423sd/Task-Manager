using Domain.Abstractions;
using Domain.Entities.DbEntities;
using Infrastracture.Context;
using Microsoft.EntityFrameworkCore;


namespace Infrastracture.Repositories
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(ApplicationContext context) : base(context) { }

        public async Task<User> GetByEmailAsync(string email)
        {
            return await _dbSet
                .FirstOrDefaultAsync(x => x.Email == email);
        }

        public async Task<User> GetByUserNameAsync(string userName)
        {
            return await _dbSet
                .FirstOrDefaultAsync(x => x.UserName == userName);
        }
    }
}
