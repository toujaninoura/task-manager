using TaskManager.Domain.Enums;

namespace TaskManager.Application.DTOs;

public class TaskQueryParams
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Search { get; set; }
    public string SortBy { get; set; } = "createdAt";
    public string SortDir { get; set; } = "desc";
    public TaskItemStatus? Status { get; set; }
    public TaskPriority? Priority { get; set; }
}
