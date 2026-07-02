using TaskManager.Domain.Enums;

namespace TaskManager.Domain.Entities;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.User;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<TaskItem> TasksCreated { get; set; } = new List<TaskItem>();
    public ICollection<TaskItem> TasksAssigned { get; set; } = new List<TaskItem>();
    public ICollection<Session> Sessions { get; set; } = new List<Session>();
}