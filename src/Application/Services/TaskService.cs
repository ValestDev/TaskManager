using TaskManager.Application.DTOs;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Enums;

namespace TaskManager.Application.Services;

public class TaskService : ITaskService
{
    private readonly ITaskRepository _taskRepository;
    private readonly IUserRepository _userRepository;
    private readonly IAuditService _auditService;

    public TaskService(ITaskRepository taskRepository, IUserRepository userRepository, IAuditService auditService)
    {
        _taskRepository = taskRepository;
        _userRepository = userRepository;
        _auditService = auditService;
    }

    public async Task<PagedResultDto<TaskItemDto>> GetPagedAsync(
        int page, int pageSize, TaskItemStatus? status, Guid? assignedToId)
    {
        var (items, total) = await _taskRepository.GetPagedAsync(page, pageSize, status, assignedToId);

        return new PagedResultDto<TaskItemDto>
        {
            Items = items.Select(MapToDto),
            Total = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<TaskItemDto> GetByIdAsync(Guid id)
    {
        var task = await _taskRepository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("Tarea no encontrada.");

        return MapToDto(task);
    }

    public async Task<TaskItemDto> CreateAsync(CreateTaskDto dto, Guid createdById)
    {
        if (dto.AssignedToId.HasValue)
        {
            var assignee = await _userRepository.GetByIdAsync(dto.AssignedToId.Value);
            if (assignee is null || !assignee.IsActive)
            {
                throw new InvalidOperationException("El usuario asignado no existe o está inactivo.");
            }
        }

        var task = new TaskItem
        {
            Title = dto.Title,
            Description = dto.Description,
            AssignedToId = dto.AssignedToId,
            CreatedById = createdById,
            Status = TaskItemStatus.Pendiente
        };

        await _taskRepository.AddAsync(task);
        await _taskRepository.SaveChangesAsync();

        var created = await _taskRepository.GetByIdAsync(task.Id)
            ?? throw new InvalidOperationException("Error al recuperar la tarea recién creada.");

        await _auditService.LogAsync(AuditAction.TaskCreated, createdById, null, $"Tarea creada: {task.Title}");

        if (task.AssignedToId.HasValue)
        {
            await _auditService.LogAsync(AuditAction.TaskAssigned, createdById, null,
                $"Tarea '{task.Title}' asignada al usuario {task.AssignedToId}");
        }

        return MapToDto(created);
    }

    public async Task<TaskItemDto> UpdateAsync(Guid id, UpdateTaskDto dto, Guid currentUserId, bool isAdmin)
    {
        var task = await _taskRepository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("Tarea no encontrada.");

        var isCreator = task.CreatedById == currentUserId;
        var isAssignee = task.AssignedToId == currentUserId;

        if (!isAdmin && !isCreator && !isAssignee)
        {
            throw new UnauthorizedAccessException("No tienes permiso para editar esta tarea.");
        }

        if (dto.AssignedToId.HasValue && dto.AssignedToId != task.AssignedToId)
        {
            var assignee = await _userRepository.GetByIdAsync(dto.AssignedToId.Value);
            if (assignee is null || !assignee.IsActive)
            {
                throw new InvalidOperationException("El usuario asignado no existe o está inactivo.");
            }
        }

        var previousAssignedToId = task.AssignedToId;

        task.Title = dto.Title;
        task.Description = dto.Description;
        task.Status = dto.Status;
        task.AssignedToId = dto.AssignedToId;
        task.UpdatedAt = DateTime.UtcNow;

        _taskRepository.Update(task);
        await _taskRepository.SaveChangesAsync();

        await _auditService.LogAsync(AuditAction.TaskUpdated, currentUserId, null, $"Tarea actualizada: {task.Title}");

        if (dto.AssignedToId != previousAssignedToId)
        {
            await _auditService.LogAsync(AuditAction.TaskAssigned, currentUserId, null,
                $"Tarea '{task.Title}' reasignada al usuario {dto.AssignedToId}");
        }

        return MapToDto(task);
    }

    private static TaskItemDto MapToDto(TaskItem task) => new()
    {
        Id = task.Id,
        Title = task.Title,
        Description = task.Description,
        Status = task.Status,
        AssignedToId = task.AssignedToId,
        AssignedToName = task.AssignedTo?.Name,
        CreatedById = task.CreatedById,
        CreatedByName = task.CreatedBy?.Name ?? string.Empty,
        CreatedAt = task.CreatedAt,
        UpdatedAt = task.UpdatedAt
    };
}