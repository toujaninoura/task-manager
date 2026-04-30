using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Application.Common;
using TaskManager.Application.DTOs;
using TaskManager.Application.Interfaces;

namespace TaskManager.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
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

    /// <summary>Get all tasks with pagination</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<TaskItemResponse>>), 200)]
    public async Task<IActionResult> GetAll([FromQuery] TaskQueryParams queryParams)
    {
        var result = await _taskService.GetAllAsync(queryParams);
        return Ok(ApiResponse<PagedResponse<TaskItemResponse>>.Ok(result));
    }

    /// <summary>Get task by id</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<TaskItemResponse>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _taskService.GetByIdAsync(id);
        return Ok(ApiResponse<TaskItemResponse>.Ok(result));
    }

    /// <summary>Create a new task</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<TaskItemResponse>), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Create([FromBody] CreateTaskItemRequest request)
    {
        var result = await _taskService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.Id },
            ApiResponse<TaskItemResponse>.Ok(result, "Task created successfully."));
    }

    /// <summary>Update an existing task</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<TaskItemResponse>), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateTaskItemRequest request)
    {
        var result = await _taskService.UpdateAsync(id, request);
        return Ok(ApiResponse<TaskItemResponse>.Ok(result, "Task updated successfully."));
    }

    /// <summary>Delete a task (soft delete)</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(int id)
    {
        await _taskService.DeleteAsync(id);
        return NoContent();
    }
}
