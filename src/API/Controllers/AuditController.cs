using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Application.DTOs;
using TaskManager.Application.Interfaces;

namespace TaskManager.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AuditController : ControllerBase
{
    private readonly IAuditService _auditService;

    public AuditController(IAuditService auditService)
    {
        _auditService = auditService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResultDto<AuditLogDto>>> GetPaged(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _auditService.GetPagedAsync(page, pageSize);
        return Ok(result);
    }
}