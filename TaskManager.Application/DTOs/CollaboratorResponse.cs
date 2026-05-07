using TaskManager.Domain.Enums;

namespace TaskManager.Application.DTOs;

public record CollaboratorResponse(
    int Id,
    int UserId,
    string Email,
    TaskShareRole Role,
    InvitationStatus Status,
    DateTime InvitedAt);
