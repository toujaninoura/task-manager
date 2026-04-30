using TaskManager.Application.Common;
using TaskManager.Application.DTOs;

namespace TaskManager.Application.Interfaces;

public interface ITaskService
{
    Task<TaskItemResponse> GetByIdAsync(int id);
    Task<PagedResponse<TaskItemResponse>> GetAllAsync(TaskQueryParams queryParams);
    Task<TaskItemResponse> CreateAsync(CreateTaskItemRequest request);
    Task<TaskItemResponse> UpdateAsync(int id, UpdateTaskItemRequest request);
    Task DeleteAsync(int id);
}
