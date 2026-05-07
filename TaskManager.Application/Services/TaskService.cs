using AutoMapper;
using FluentValidation;
using Microsoft.Extensions.Logging;
using TaskManager.Application.Common;
using TaskManager.Application.DTOs;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Exceptions;
using ValidationException = TaskManager.Domain.Exceptions.ValidationException;

namespace TaskManager.Application.Services;

public class TaskService : ITaskService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<TaskService> _logger;
    private readonly IValidator<CreateTaskItemRequest> _createValidator;
    private readonly IValidator<UpdateTaskItemRequest> _updateValidator;

    public TaskService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<TaskService> logger,
        IValidator<CreateTaskItemRequest> createValidator,
        IValidator<UpdateTaskItemRequest> updateValidator)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<PagedResponse<TaskItemResponse>> GetAllAsync(int userId, int page, int pageSize, CancellationToken ct = default)
    {
        var items = await _unitOfWork.Tasks.GetAllByUserIdAsync(userId, page, pageSize, ct);
        var totalCount = await _unitOfWork.Tasks.CountByUserIdAsync(userId, ct);
        var mapped = _mapper.Map<IEnumerable<TaskItemResponse>>(items);

        _logger.LogInformation("Retrieved {Count} tasks for user {UserId}", totalCount, userId);

        return PagedResponse<TaskItemResponse>.Create(mapped, page, pageSize, totalCount);
    }

    public async Task<TaskItemResponse> GetByIdAsync(int id, int userId, CancellationToken ct = default)
    {
        var taskItem = await _unitOfWork.Tasks.GetByIdAndUserIdAsync(id, userId, ct);
        if (taskItem is null)
            throw new NotFoundException(nameof(TaskItem), id);

        return _mapper.Map<TaskItemResponse>(taskItem);
    }

    public async Task<TaskItemResponse> CreateAsync(CreateTaskItemRequest request, int userId, CancellationToken ct = default)
    {
        var validation = await _createValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            throw new ValidationException(validation.Errors.Select(e => e.ErrorMessage));

        var taskItem = _mapper.Map<TaskItem>(request);
        taskItem.UserId = userId;
        taskItem.CreatedAt = DateTime.UtcNow;
        taskItem.UpdatedAt = DateTime.UtcNow;

        var created = await _unitOfWork.Tasks.CreateAsync(taskItem, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("Task created with id {TaskId} for user {UserId}", created.Id, userId);

        return _mapper.Map<TaskItemResponse>(created);
    }

    public async Task<TaskItemResponse> UpdateAsync(int id, UpdateTaskItemRequest request, int userId, CancellationToken ct = default)
    {
        var validation = await _updateValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            throw new ValidationException(validation.Errors.Select(e => e.ErrorMessage));

        var taskItem = await _unitOfWork.Tasks.GetByIdAndUserIdTrackingAsync(id, userId, ct)
            ?? throw new NotFoundException(nameof(TaskItem), id);

        _mapper.Map(request, taskItem);
        taskItem.UpdatedAt = DateTime.UtcNow;

        var updated = await _unitOfWork.Tasks.UpdateAsync(taskItem, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("Task {TaskId} updated for user {UserId}", id, userId);

        return _mapper.Map<TaskItemResponse>(updated);
    }

    public async Task DeleteAsync(int id, int userId, CancellationToken ct = default)
    {
        var taskItem = await _unitOfWork.Tasks.GetByIdAndUserIdTrackingAsync(id, userId, ct)
            ?? throw new NotFoundException(nameof(TaskItem), id);

        await _unitOfWork.Tasks.SoftDeleteAsync(taskItem, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("Task {TaskId} soft-deleted for user {UserId}", id, userId);
    }
}
