using AutoMapper;
using Microsoft.Extensions.Logging;
using TaskManager.Application.Common;
using TaskManager.Application.DTOs;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Exceptions;

namespace TaskManager.Application.Services;

public class TaskService : ITaskService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<TaskService> _logger;

    public TaskService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<TaskService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<TaskItemResponse> GetByIdAsync(int id)
    {
        var taskItem = await _unitOfWork.Tasks.GetByIdAsync(id);
        if (taskItem is null)
            throw new NotFoundException(nameof(TaskItem), id);

        return _mapper.Map<TaskItemResponse>(taskItem);
    }

    public async Task<PagedResponse<TaskItemResponse>> GetAllAsync(TaskQueryParams queryParams)
    {
        var pagedTasks = await _unitOfWork.Tasks.GetAllAsync(queryParams);
        var mappedData = _mapper.Map<IEnumerable<TaskItemResponse>>(pagedTasks.Data);

        return PagedResponse<TaskItemResponse>.Create(
            mappedData,
            pagedTasks.Page,
            pagedTasks.PageSize,
            pagedTasks.TotalCount
        );
    }

    public async Task<TaskItemResponse> CreateAsync(CreateTaskItemRequest request)
    {
        var taskItem = _mapper.Map<TaskItem>(request);
        taskItem.CreatedAt = DateTime.UtcNow;
        taskItem.UpdatedAt = DateTime.UtcNow;

        var created = await _unitOfWork.Tasks.CreateAsync(taskItem);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Task created with id {TaskId}", created.Id);

        return _mapper.Map<TaskItemResponse>(created);
    }

    public async Task<TaskItemResponse> UpdateAsync(int id, UpdateTaskItemRequest request)
    {
        var taskItem = await _unitOfWork.Tasks.GetByIdAsync(id);
        if (taskItem is null)
            throw new NotFoundException(nameof(TaskItem), id);

        taskItem.Title = request.Title;
        taskItem.Description = request.Description;
        taskItem.Status = request.Status;
        taskItem.Priority = request.Priority;
        taskItem.DueDate = request.DueDate;
        taskItem.UpdatedAt = DateTime.UtcNow;

        var saved = await _unitOfWork.Tasks.UpdateAsync(taskItem);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Task updated with id {TaskId}", id);

        return _mapper.Map<TaskItemResponse>(saved);
    }

    public async Task DeleteAsync(int id)
    {
        var taskItem = await _unitOfWork.Tasks.GetByIdAsync(id);
        if (taskItem is null)
            throw new NotFoundException(nameof(TaskItem), id);

        await _unitOfWork.Tasks.DeleteAsync(taskItem);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Task deleted with id {TaskId}", id);
    }
}
