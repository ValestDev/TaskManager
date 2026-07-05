namespace TaskManager.Application.Interfaces;

public interface IAuditContext
{
    string? IpAddress { get; }
    string? Endpoint { get; }
}