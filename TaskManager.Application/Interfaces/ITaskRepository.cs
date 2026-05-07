using TaskManager.Application.Common;
using TaskManager.Application.DTOs;
using TaskManager.Domain.Entities;

namespace TaskManager.Application.Interfaces;

public interface ITaskRepository
{
    Task<IEnumerable<TaskItem>> GetAllByUserIdAsync(int userId, int page, int pageSize, CancellationToken ct = default);
    Task<int> CountByUserIdAsync(int userId, CancellationToken ct = default);
    Task<TaskItem?> GetByIdAndUserIdAsync(int id, int userId, CancellationToken ct = default);
    Task<TaskItem?> GetByIdAndUserIdTrackingAsync(int id, int userId, CancellationToken ct = default);
    Task<TaskItem> CreateAsync(TaskItem task, CancellationToken ct = default);
    Task<TaskItem> UpdateAsync(TaskItem task, CancellationToken ct = default);
    Task SoftDeleteAsync(TaskItem task, CancellationToken ct = default);
}
