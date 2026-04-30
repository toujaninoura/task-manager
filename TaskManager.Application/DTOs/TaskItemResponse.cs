using TaskManager.Domain.Enums;

namespace TaskManager.Application.DTOs;

public record TaskItemResponse(
    int Id,
    string Title,
    string? Description,
    TaskItemStatus Status,
    string StatusLabel,
    TaskPriority Priority,
    string PriorityLabel,
    DateTime? DueDate,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
