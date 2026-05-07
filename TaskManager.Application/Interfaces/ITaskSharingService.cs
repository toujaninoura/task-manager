using TaskManager.Application.DTOs;

namespace TaskManager.Application.Interfaces;

public interface ITaskSharingService
{
    Task<CollaboratorResponse> InviteCollaboratorAsync(int taskId, InviteCollaboratorRequest request, int ownerUserId, CancellationToken ct = default);
    Task AcceptInvitationAsync(int taskId, int userId, CancellationToken ct = default);
    Task RejectInvitationAsync(int taskId, int userId, CancellationToken ct = default);
    Task RemoveCollaboratorAsync(int taskId, int targetUserId, int requestingUserId, CancellationToken ct = default);
    Task<IEnumerable<CollaboratorResponse>> GetCollaboratorsAsync(int taskId, int requestingUserId, CancellationToken ct = default);
    Task<IEnumerable<InvitationResponse>> GetSharedTasksAsync(int userId, CancellationToken ct = default);
    Task<IEnumerable<InvitationResponse>> GetPendingInvitationsAsync(int userId, CancellationToken ct = default);
}
