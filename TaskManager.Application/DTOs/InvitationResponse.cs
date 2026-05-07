using TaskManager.Domain.Enums;

namespace TaskManager.Application.DTOs;

public record InvitationResponse(
    int Id,
    int TaskId,
    string TaskTitle,
    string InvitedByEmail,
    TaskShareRole Role,
    DateTime InvitedAt);
