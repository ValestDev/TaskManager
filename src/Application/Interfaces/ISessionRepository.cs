using TaskManager.Domain.Entities;

namespace TaskManager.Application.Interfaces;

public interface ISessionRepository
{
    Task AddAsync(Session session);
    Task<Session?> GetActiveSessionAsync(Guid userId);
    Task<IEnumerable<Session>> GetOnlineSessionsAsync();
    void Update(Session session);
    Task SaveChangesAsync();
}