using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Application.Common;
using TaskManager.Application.DTOs;
using TaskManager.Application.Interfaces;

namespace TaskManager.API.Controllers;

[ApiController]
[Route("api/v1/tasks")]
[Authorize]
public class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;
    private readonly ILogger<TasksController> _logger;

    public TasksController(ITaskService taskService, ILogger<TasksController> logger)
    {
        _taskService = taskService;
        _logger = logger;
    }

    private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>Get all tasks for the authenticated user with pagination</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<TaskItemResponse>>), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken ct = default)
    {
        var result = await _taskService.GetAllAsync(GetUserId(), page, pageSize, ct);
        return Ok(ApiResponse<PagedResponse<TaskItemResponse>>.Ok(result));
    }

    /// <summary>Get task by id for the authenticated user</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<TaskItemResponse>), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct = default)
    {
        var result = await _taskService.GetByIdAsync(id, GetUserId(), ct);
        return Ok(ApiResponse<TaskItemResponse>.Ok(result));
    }

    /// <summary>Create a new task for the authenticated user</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<TaskItemResponse>), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> Create([FromBody] CreateTaskItemRequest request, CancellationToken ct = default)
    {
        var result = await _taskService.CreateAsync(request, GetUserId(), ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id },
            ApiResponse<TaskItemResponse>.Ok(result, "Task created successfully."));
    }

    /// <summary>Update an existing task for the authenticated user</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<TaskItemResponse>), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateTaskItemRequest request, CancellationToken ct = default)
    {
        var result = await _taskService.UpdateAsync(id, request, GetUserId(), ct);
        return Ok(ApiResponse<TaskItemResponse>.Ok(result, "Task updated successfully."));
    }

    /// <summary>Soft delete a task for the authenticated user</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct = default)
    {
        await _taskService.DeleteAsync(id, GetUserId(), ct);
        return NoContent();
    }
}
