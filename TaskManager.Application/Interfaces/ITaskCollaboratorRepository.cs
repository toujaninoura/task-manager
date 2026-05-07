using TaskManager.Domain.Entities;
using TaskManager.Domain.Enums;

namespace TaskManager.Application.Interfaces;

public interface ITaskCollaboratorRepository
{
    Task<TaskCollaborator?> GetByTaskAndUserAsync(int taskId, int userId, CancellationToken ct = default);
    Task<TaskCollaborator?> GetByTaskAndUserTrackingAsync(int taskId, int userId, CancellationToken ct = default);
    Task<IEnumerable<TaskCollaborator>> GetByTaskIdAsync(int taskId, CancellationToken ct = default);
    Task<IEnumerable<TaskCollaborator>> GetSharedWithUserAsync(int userId, CancellationToken ct = default);
    Task<IEnumerable<TaskCollaborator>> GetPendingForUserAsync(int userId, CancellationToken ct = default);
    Task<TaskCollaborator> CreateAsync(TaskCollaborator collaborator, CancellationToken ct = default);
    Task UpdateAsync(TaskCollaborator collaborator, CancellationToken ct = default);
    Task DeleteAsync(TaskCollaborator collaborator, CancellationToken ct = default);
    Task<bool> IsAcceptedCollaboratorAsync(int taskId, int userId, CancellationToken ct = default);
    Task<TaskShareRole?> GetUserRoleAsync(int taskId, int userId, CancellationToken ct = default);
}
