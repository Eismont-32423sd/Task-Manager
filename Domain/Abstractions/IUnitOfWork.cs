namespace Domain.Abstractions
{
    public interface IUnitOfWork
    {
        IUserRepository UserRepository { get; }
        IProjectRepository ProjectRepository { get; }
        IStageRepository StageRepository { get; }
        IStageAssignmentRepository StageAssignmentRepository { get; }
        ICommitRepository CommitRepository { get; }
        Task SaveChangesAsync();
    }
}
