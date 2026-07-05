using TaskManager.Domain.Entities;
using TaskManager.Domain.Enums;

namespace TaskManager.Application.Interfaces;

public interface ITaskRepository
{
    Task<TaskItem?> GetByIdAsync(Guid id);
    Task<(IEnumerable<TaskItem> Items, int Total)> GetPagedAsync(
        int page, int pageSize, TaskItemStatus? status, Guid? assignedToId);
    Task AddAsync(TaskItem task);
    void Update(TaskItem task);
    Task SaveChangesAsync();
}