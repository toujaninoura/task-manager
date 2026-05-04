using TaskManager.Application.Common;
using TaskManager.Application.DTOs;
using TaskManager.Domain.Entities;

namespace TaskManager.Application.Interfaces;

public interface ITaskRepository
{
    Task<TaskItem?> GetByIdAsync(int id);
    Task<TaskItem?> GetByIdNoTrackingAsync(int id);
    Task<PagedResponse<TaskItem>> GetAllAsync(TaskQueryParams queryParams);
    Task<TaskItem> CreateAsync(TaskItem taskItem);
    Task<TaskItem> UpdateAsync(TaskItem taskItem);
    Task DeleteAsync(TaskItem taskItem);
    Task<bool> ExistsAsync(int id);
}
