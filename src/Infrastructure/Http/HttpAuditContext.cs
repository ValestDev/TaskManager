using Microsoft.AspNetCore.Http;
using TaskManager.Application.Interfaces;

namespace TaskManager.Infrastructure.Http;

public class HttpAuditContext : IAuditContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpAuditContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? IpAddress =>
        _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();

    public string? Endpoint =>
        _httpContextAccessor.HttpContext is null
            ? null
            : $"{_httpContextAccessor.HttpContext.Request.Method} {_httpContextAccessor.HttpContext.Request.Path}";
}