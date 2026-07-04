using TaskManager.Domain.Entities;

namespace TaskManager.Application.Interfaces;

public interface IAuditLogRepository
{
    Task AddAsync(AuditLog log);
    Task<(IEnumerable<AuditLog> Items, int Total)> GetPagedAsync(int page, int pageSize);
    Task SaveChangesAsync();
}