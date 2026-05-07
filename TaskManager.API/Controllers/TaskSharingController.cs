using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Application.Common;
using TaskManager.Application.DTOs;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Exceptions;

namespace TaskManager.API.Controllers;

[ApiController]
[Route("api/v1/tasks")]
[Authorize]
public class TaskSharingController : ControllerBase
{
    private readonly ITaskSharingService _sharingService;
    private readonly ILogger<TaskSharingController> _logger;

    public TaskSharingController(ITaskSharingService sharingService, ILogger<TaskSharingController> logger)
    {
        _sharingService = sharingService;
        _logger = logger;
    }

    private int GetUserId()
    {
        if (int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id))
            return id;
        throw new UnauthorizedException();
    }

    /// <summary>Invite a collaborator to a task (Owner only)</summary>
    [HttpPost("{id:int}/collaborators")]
    [ProducesResponseType(typeof(ApiResponse<CollaboratorResponse>), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> InviteCollaborator(int id, [FromBody] InviteCollaboratorRequest request, CancellationToken ct = default)
    {
        var result = await _sharingService.InviteCollaboratorAsync(id, request, GetUserId(), ct);
        return CreatedAtAction(nameof(GetCollaborators), new { id }, ApiResponse<CollaboratorResponse>.Ok(result, "Collaborator invited successfully."));
    }

    /// <summary>Remove a collaborator from a task</summary>
    [HttpDelete("{id:int}/collaborators/{userId:int}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> RemoveCollaborator(int id, int userId, CancellationToken ct = default)
    {
        await _sharingService.RemoveCollaboratorAsync(id, userId, GetUserId(), ct);
        return NoContent();
    }

    /// <summary>Get all collaborators for a task</summary>
    [HttpGet("{id:int}/collaborators")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CollaboratorResponse>>), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetCollaborators(int id, CancellationToken ct = default)
    {
        var result = await _sharingService.GetCollaboratorsAsync(id, GetUserId(), ct);
        return Ok(ApiResponse<IEnumerable<CollaboratorResponse>>.Ok(result));
    }

    /// <summary>Get tasks shared with the authenticated user (accepted)</summary>
    [HttpGet("shared")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<InvitationResponse>>), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> GetSharedTasks(CancellationToken ct = default)
    {
        var result = await _sharingService.GetSharedTasksAsync(GetUserId(), ct);
        return Ok(ApiResponse<IEnumerable<InvitationResponse>>.Ok(result));
    }

    /// <summary>Get pending invitations for the authenticated user</summary>
    [HttpGet("shared/pending")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<InvitationResponse>>), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> GetPendingInvitations(CancellationToken ct = default)
    {
        var result = await _sharingService.GetPendingInvitationsAsync(GetUserId(), ct);
        return Ok(ApiResponse<IEnumerable<InvitationResponse>>.Ok(result));
    }

    /// <summary>Accept a task invitation</summary>
    [HttpPost("{id:int}/collaborators/accept")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> AcceptInvitation(int id, CancellationToken ct = default)
    {
        await _sharingService.AcceptInvitationAsync(id, GetUserId(), ct);
        return Ok(ApiResponse<string>.Ok(string.Empty, "Invitation accepted."));
    }

    /// <summary>Reject a task invitation</summary>
    [HttpPost("{id:int}/collaborators/reject")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> RejectInvitation(int id, CancellationToken ct = default)
    {
        await _sharingService.RejectInvitationAsync(id, GetUserId(), ct);
        return Ok(ApiResponse<string>.Ok(string.Empty, "Invitation rejected."));
    }
}
