using Microsoft.EntityFrameworkCore;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Enums;
using TaskManager.Infrastructure.Data;

namespace TaskManager.Infrastructure.Repositories;

public class TaskCollaboratorRepository : ITaskCollaboratorRepository
{
    private readonly AppDbContext _context;

    public TaskCollaboratorRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<TaskCollaborator?> GetByTaskAndUserAsync(int taskId, int userId, CancellationToken ct = default)
    {
        return await _context.TaskCollaborators
            .AsNoTracking()
            .FirstOrDefaultAsync(tc => tc.TaskId == taskId && tc.UserId == userId, ct);
    }

    public async Task<IEnumerable<TaskCollaborator>> GetByTaskIdAsync(int taskId, CancellationToken ct = default)
    {
        return await _context.TaskCollaborators
            .AsNoTracking()
            .Include(tc => tc.User)
            .Include(tc => tc.InvitedByUser)
            .Where(tc => tc.TaskId == taskId)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<TaskCollaborator>> GetSharedWithUserAsync(int userId, CancellationToken ct = default)
    {
        return await _context.TaskCollaborators
            .AsNoTracking()
            .Include(tc => tc.Task)
            .Include(tc => tc.InvitedByUser)
            .Where(tc => tc.UserId == userId && tc.Status == InvitationStatus.Accepted)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<TaskCollaborator>> GetPendingForUserAsync(int userId, CancellationToken ct = default)
    {
        return await _context.TaskCollaborators
            .AsNoTracking()
            .Include(tc => tc.Task)
            .Include(tc => tc.InvitedByUser)
            .Where(tc => tc.UserId == userId && tc.Status == InvitationStatus.Pending)
            .ToListAsync(ct);
    }

    public async Task<TaskCollaborator> CreateAsync(TaskCollaborator collaborator, CancellationToken ct = default)
    {
        await _context.TaskCollaborators.AddAsync(collaborator, ct);
        return collaborator;
    }

    public Task UpdateAsync(TaskCollaborator collaborator, CancellationToken ct = default)
    {
        _context.TaskCollaborators.Update(collaborator);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(TaskCollaborator collaborator, CancellationToken ct = default)
    {
        _context.TaskCollaborators.Remove(collaborator);
        return Task.CompletedTask;
    }

    public async Task<bool> IsAcceptedCollaboratorAsync(int taskId, int userId, CancellationToken ct = default)
    {
        return await _context.TaskCollaborators
            .AsNoTracking()
            .AnyAsync(tc => tc.TaskId == taskId && tc.UserId == userId && tc.Status == InvitationStatus.Accepted, ct);
    }

    public async Task<TaskShareRole?> GetUserRoleAsync(int taskId, int userId, CancellationToken ct = default)
    {
        var collab = await _context.TaskCollaborators
            .AsNoTracking()
            .FirstOrDefaultAsync(tc => tc.TaskId == taskId && tc.UserId == userId && tc.Status == InvitationStatus.Accepted, ct);
        return collab?.Role;
    }
}
