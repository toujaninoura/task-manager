using TaskManager.Domain.Enums;

namespace TaskManager.Application.DTOs;

public record InviteCollaboratorRequest(string Email, TaskShareRole Role);
