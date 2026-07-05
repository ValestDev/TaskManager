using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaskManager.Application.DTOs;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Enums;

namespace TaskManager.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;

    public TasksController(ITaskService taskService)
    {
        _taskService = taskService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResultDto<TaskItemDto>>> GetPaged(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] TaskItemStatus? status = null,
        [FromQuery] bool onlyMine = false)
    {
        Guid? assignedToId = onlyMine ? GetCurrentUserId() : null;

        var result = await _taskService.GetPagedAsync(page, pageSize, status, assignedToId);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TaskItemDto>> GetById(Guid id)
    {
        var result = await _taskService.GetByIdAsync(id);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<TaskItemDto>> Create([FromBody] CreateTaskDto dto)
    {
        var result = await _taskService.CreateAsync(dto, GetCurrentUserId());
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<TaskItemDto>> Update(Guid id, [FromBody] UpdateTaskDto dto)
    {
        var isAdmin = User.IsInRole("Admin");
        var result = await _taskService.UpdateAsync(id, dto, GetCurrentUserId(), isAdmin);
        return Ok(result);
    }

    private Guid GetCurrentUserId()
    {
        var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
                       ?? User.FindFirstValue("sub");

        if (idClaim is null || !Guid.TryParse(idClaim, out var id))
        {
            throw new UnauthorizedAccessException("No se pudo identificar al usuario actual.");
        }

        return id;
    }
}