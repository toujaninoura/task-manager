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
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
    }

    public async Task<int> CountByUserIdAsync(int userId, CancellationToken ct = default)
    {
        return await _context.Tasks
            .AsNoTracking()
            .CountAsync(t => t.UserId == userId, ct);
    }

    public async Task<TaskItem?> GetByIdAndUserIdAsync(int id, int userId, CancellationToken ct = default)
    {
        return await _context.Tasks
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId, ct);
    }

    public async Task<TaskItem> CreateAsync(TaskItem task, CancellationToken ct = default)
    {
        await _context.Tasks.AddAsync(task, ct);
        return task;
    }

    public async Task UpdateAsync(TaskItem task, CancellationToken ct = default)
    {
        var existing = await _context.Tasks
            .FirstOrDefaultAsync(t => t.Id == task.Id && t.UserId == task.UserId, ct);

        if (existing is null)
            return;

        existing.Title = task.Title;
        existing.Description = task.Description;
        existing.Status = task.Status;
        existing.Priority = task.Priority;
        existing.DueDate = task.DueDate;
        existing.UpdatedAt = task.UpdatedAt;
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
