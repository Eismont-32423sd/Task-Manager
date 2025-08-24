using Domain.Abstractions;
using Infrastracture.Context;
using Infrastracture.Repositories;

namespace Infrastracture.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private IUserRepository? _userRepository;
        private IProjectRepository? _projectRepository;
        private readonly ApplicationContext _context;

        public UnitOfWork(ApplicationContext context)
        {
            _context = context;
        }
        public IUserRepository UserRepository
        {
            get
            {
                if( _userRepository == null )
                {
                    _userRepository = new UserRepository( _context );
                    return _userRepository;
                }
                return _userRepository;
            }
        }

        public IProjectRepository ProjectRepository
        {
            get
            {
                if( _projectRepository == null )
                {
                    _projectRepository = new ProjectRepository( _context );
                    return _projectRepository;
                }
                return _projectRepository;
            }
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
