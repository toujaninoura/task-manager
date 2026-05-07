using TaskManager.Application.Interfaces;
using TaskManager.Infrastructure.Data;

namespace TaskManager.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    public ITaskRepository Tasks { get; }
    public ITaskCollaboratorRepository Collaborators { get; }

    public UnitOfWork(AppDbContext context, ITaskRepository taskRepository, ITaskCollaboratorRepository collaboratorRepository)
    {
        _context = context;
        Tasks = taskRepository;
        Collaborators = collaboratorRepository;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
