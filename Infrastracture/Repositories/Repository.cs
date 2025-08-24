using Domain.Abstractions;
using Infrastracture.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastracture.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly ApplicationContext _context;
        protected DbSet<T> _dbSet;

        public Repository(ApplicationContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        public async Task AddAsync(T entity)
        {
             await _dbSet.AddAsync(entity);
        }

        public void Delete(T entity)
        {
             _dbSet.Remove(entity);
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public async Task<T> GetByIdAsync(Guid Id)
        {
            return await _dbSet.FindAsync(Id);
        }

        public void Update(T entity)
        {
            _dbSet.Update(entity);
        }
    }
}
