using Domain.Abstractions;
using Infrastracture.Context;
using Infrastracture.Repositories;

namespace Infrastracture.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private IUserRepository? _userRepository;
        private IProjectRepository? _projectRepository;
        private IStageRepository? _stageRepository;
        private IStageAssignmentRepository? _stageAssignmentRepository;
        private ICommitRepository? _commitRepository;
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

        public ICommitRepository CommitRepository
        {
            get
            {
                if( _commitRepository == null)
                {
                    _commitRepository = new CommitRepository( _context );
                    return _commitRepository;
                }
                return _commitRepository;
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

        public IStageRepository StageRepository
        {
            get
            {
                if( _stageRepository == null )
                {
                    _stageRepository = new StageRepository( _context );
                    return _stageRepository;
                }
                return _stageRepository;
            }
        }

        public IStageAssignmentRepository StageAssignmentRepository
        {
            get
            {
                if(_stageAssignmentRepository == null)
                {
                    _stageAssignmentRepository = new StageAssignmentRepository( _context );
                    return _stageAssignmentRepository;
                }

                return _stageAssignmentRepository;
            }
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
