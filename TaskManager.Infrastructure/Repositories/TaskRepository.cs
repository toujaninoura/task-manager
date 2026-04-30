using Microsoft.EntityFrameworkCore;
using TaskManager.Application.Common;
using TaskManager.Application.DTOs;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Entities;
using TaskManager.Infrastructure.Data;

namespace TaskManager.Infrastructure.Repositories;

public class TaskRepository : ITaskRepository
{
    private readonly AppDbContext _context;

    public TaskRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<TaskItem?> GetByIdAsync(int id)
    {
        return await _context.Tasks
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<PagedResponse<TaskItem>> GetAllAsync(TaskQueryParams queryParams)
    {
        var query = _context.Tasks.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(queryParams.Search))
            query = query.Where(t => t.Title.Contains(queryParams.Search) ||
                                     (t.Description != null && t.Description.Contains(queryParams.Search)));

        if (queryParams.Status.HasValue)
            query = query.Where(t => t.Status == queryParams.Status.Value);

        if (queryParams.Priority.HasValue)
            query = query.Where(t => t.Priority == queryParams.Priority.Value);

        query = queryParams.SortBy.ToLower() switch
        {
            "title" => queryParams.SortDir == "asc" ? query.OrderBy(t => t.Title) : query.OrderByDescending(t => t.Title),
            "priority" => queryParams.SortDir == "asc" ? query.OrderBy(t => t.Priority) : query.OrderByDescending(t => t.Priority),
            "duedate" => queryParams.SortDir == "asc" ? query.OrderBy(t => t.DueDate) : query.OrderByDescending(t => t.DueDate),
            _ => queryParams.SortDir == "asc" ? query.OrderBy(t => t.CreatedAt) : query.OrderByDescending(t => t.CreatedAt)
        };

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((queryParams.Page - 1) * queryParams.PageSize)
            .Take(queryParams.PageSize)
            .ToListAsync();

        return PagedResponse<TaskItem>.Create(items, queryParams.Page, queryParams.PageSize, totalCount);
    }

    public async Task<TaskItem> CreateAsync(TaskItem taskItem)
    {
        await _context.Tasks.AddAsync(taskItem);
        return taskItem;
    }

    public async Task<TaskItem> UpdateAsync(TaskItem taskItem)
    {
        _context.Tasks.Update(taskItem);
        return await Task.FromResult(taskItem);
    }

    public async Task DeleteAsync(TaskItem taskItem)
    {
        taskItem.IsDeleted = true;
        taskItem.DeletedAt = DateTime.UtcNow;
        _context.Tasks.Update(taskItem);
        await Task.CompletedTask;
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Tasks.AnyAsync(t => t.Id == id);
    }
}
