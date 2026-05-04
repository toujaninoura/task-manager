using TaskManager.Application.Common;
using TaskManager.Application.DTOs;

namespace TaskManager.Application.Interfaces;

public interface ITaskService
{
    Task<PagedResponse<TaskItemResponse>> GetAllAsync(int userId, int page, int pageSize, CancellationToken ct = default);
    Task<TaskItemResponse> GetByIdAsync(int id, int userId, CancellationToken ct = default);
    Task<TaskItemResponse> CreateAsync(CreateTaskItemRequest request, int userId, CancellationToken ct = default);
    Task<TaskItemResponse> UpdateAsync(int id, UpdateTaskItemRequest request, int userId, CancellationToken ct = default);
    Task DeleteAsync(int id, int userId, CancellationToken ct = default);
}
