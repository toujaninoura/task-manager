namespace TaskManager.Domain.Entities;

public class User
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
    public ICollection<TaskCollaborator> CollaborationsReceived { get; set; } = new List<TaskCollaborator>();
    public ICollection<TaskCollaborator> CollaborationsGiven { get; set; } = new List<TaskCollaborator>();
}
