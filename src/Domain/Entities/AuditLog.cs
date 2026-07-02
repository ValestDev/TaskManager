using TaskManager.Domain.Enums;

namespace TaskManager.Domain.Entities;

public class AuditLog
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid? UserId { get; set; }
    public string? UserEmail { get; set; }
    public string Action { get; set; } = string.Empty; // login, logout, user_created, etc.
    public string? Details { get; set; }
    public string? IpAddress { get; set; }
    public string? Endpoint { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}