namespace TaskManager.Application.Interfaces;

public interface IUnitOfWork : IDisposable
{
    ITaskRepository Tasks { get; }
    ITaskCollaboratorRepository Collaborators { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
