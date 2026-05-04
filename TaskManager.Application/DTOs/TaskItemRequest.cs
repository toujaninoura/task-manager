using TaskManager.Domain.Enums;

namespace TaskManager.Application.DTOs;

public record CreateTaskItemRequest(
    string Title,
    string? Description,
    TaskItemStatus Status,
    TaskPriority Priority,
    DateTime? DueDate
);

public record UpdateTaskItemRequest(
    string Title,
    string? Description,
    TaskItemStatus Status,
    TaskPriority Priority,
    DateTime? DueDate
);
