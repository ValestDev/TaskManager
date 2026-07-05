using TaskManager.Application.DTOs;
using TaskManager.Domain.Enums;

namespace TaskManager.Application.Interfaces;

public interface ITaskService
{
    Task<PagedResultDto<TaskItemDto>> GetPagedAsync(
        int page, int pageSize, TaskItemStatus? status, Guid? assignedToId);

    Task<TaskItemDto> GetByIdAsync(Guid id);

    Task<TaskItemDto> CreateAsync(CreateTaskDto dto, Guid createdById);

    Task<TaskItemDto> UpdateAsync(Guid id, UpdateTaskDto dto, Guid currentUserId, bool isAdmin);
}