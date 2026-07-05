using TaskManager.Application.DTOs;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Enums;

namespace TaskManager.Application.Services;

public class AuditService : IAuditService
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IAuditContext _auditContext;

    public AuditService(IAuditLogRepository auditLogRepository, IAuditContext auditContext)
    {
        _auditLogRepository = auditLogRepository;
        _auditContext = auditContext;
    }

    public async Task LogAsync(AuditAction action, Guid? userId, string? userEmail, string? details)
    {
        var log = new AuditLog
        {
            Action = action,
            UserId = userId,
            UserEmail = userEmail,
            Details = details,
            IpAddress = _auditContext.IpAddress,
            Endpoint = _auditContext.Endpoint
        };

        await _auditLogRepository.AddAsync(log);
        await _auditLogRepository.SaveChangesAsync();
    }

    public async Task<PagedResultDto<AuditLogDto>> GetPagedAsync(int page, int pageSize)
    {
        var (items, total) = await _auditLogRepository.GetPagedAsync(page, pageSize);

        return new PagedResultDto<AuditLogDto>
        {
            Items = items.Select(MapToDto),
            Total = total,
            Page = page,
            PageSize = pageSize
        };
    }

    private static AuditLogDto MapToDto(AuditLog log) => new()
    {
        Id = log.Id,
        UserId = log.UserId,
        UserEmail = log.UserEmail,
        Action = log.Action,
        Details = log.Details,
        IpAddress = log.IpAddress,
        Endpoint = log.Endpoint,
        Timestamp = log.Timestamp
    };
}