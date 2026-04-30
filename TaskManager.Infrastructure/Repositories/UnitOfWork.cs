using TaskManager.Application.Interfaces;
using TaskManager.Infrastructure.Data;

namespace TaskManager.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private ITaskRepository? _tasks;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    public ITaskRepository Tasks => _tasks ??= new TaskRepository(_context);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
