using TaskManager.Application.DTOs;
using TaskManager.Domain.Enums;

namespace TaskManager.Application.Interfaces;

public interface IAuditService
{
    Task LogAsync(AuditAction action, Guid? userId, string? userEmail, string? details);
    Task<PagedResultDto<AuditLogDto>> GetPagedAsync(int page, int pageSize);
}