using Microsoft.EntityFrameworkCore;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Entities;
using TaskManager.Infrastructure.Data;

namespace TaskManager.Infrastructure.Repositories;

public class TaskRepository : ITaskRepository
{
    private readonly AppDbContext _context;

    public TaskRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<TaskItem>> GetAllByUserIdAsync(int userId, int page, int pageSize, CancellationToken ct = default)
    {
        return await _context.Tasks
            .AsNoTracking()
            .Where(t => t.UserId == userId && !t.IsDeleted)
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
    }

    public async Task<int> CountByUserIdAsync(int userId, CancellationToken ct = default)
    {
        return await _context.Tasks
            .AsNoTracking()
            .CountAsync(t => t.UserId == userId && !t.IsDeleted, ct);
    }

    public async Task<TaskItem?> GetByIdAndUserIdAsync(int id, int userId, CancellationToken ct = default)
    {
        return await _context.Tasks
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId && !t.IsDeleted, ct);
    }

    public async Task<TaskItem?> GetByIdAndUserIdTrackingAsync(int id, int userId, CancellationToken ct = default)
    {
        return await _context.Tasks
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId && !t.IsDeleted, ct);
    }

    public async Task<TaskItem> CreateAsync(TaskItem task, CancellationToken ct = default)
    {
        await _context.Tasks.AddAsync(task, ct);
        return task;
    }

    public Task<TaskItem> UpdateAsync(TaskItem task, CancellationToken ct = default)
    {
        _context.Entry(task).State = EntityState.Modified;
        return Task.FromResult(task);
    }

    public async Task SoftDeleteAsync(int id, int userId, CancellationToken ct = default)
    {
        var task = await _context.Tasks
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId, ct);

        if (task is null)
            return;

        task.IsDeleted = true;
        task.DeletedAt = DateTime.UtcNow;
    }
}
