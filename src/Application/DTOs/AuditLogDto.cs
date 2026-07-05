using TaskManager.Domain.Enums;

namespace TaskManager.Application.DTOs;

public class AuditLogDto
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
    public string? UserEmail { get; set; }
    public AuditAction Action { get; set; }
    public string? Details { get; set; }
    public string? IpAddress { get; set; }
    public string? Endpoint { get; set; }
    public DateTime Timestamp { get; set; }
}