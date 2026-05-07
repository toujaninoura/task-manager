using TaskManager.Domain.Enums;

namespace TaskManager.Domain.Entities;

public class TaskCollaborator
{
    public int Id { get; set; }
    public int TaskId { get; set; }
    public TaskItem Task { get; set; } = null!;
    public int InvitedUserId { get; set; }
    public User InvitedUser { get; set; } = null!;
    public int InvitedByUserId { get; set; }
    public User InvitedByUser { get; set; } = null!;
    public TaskShareRole Role { get; set; }
    public InvitationStatus Status { get; set; } = InvitationStatus.Pending;
    public DateTime InvitedAt { get; set; } = DateTime.UtcNow;
    public DateTime? RespondedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
