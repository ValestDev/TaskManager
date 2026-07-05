using Microsoft.EntityFrameworkCore;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Entities;
using TaskManager.Infrastructure.Persistence;

namespace TaskManager.Infrastructure.Repositories;

public class SessionRepository : ISessionRepository
{
    private readonly AppDbContext _context;

    public SessionRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Session session)
    {
        await _context.Sessions.AddAsync(session);
    }

    public async Task<Session?> GetActiveSessionAsync(Guid userId)
    {
        return await _context.Sessions
            .Where(s => s.UserId == userId && s.IsOnline)
            .OrderByDescending(s => s.LoginAt)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Session>> GetOnlineSessionsAsync()
    {
        return await _context.Sessions
            .Include(s => s.User)
            .Where(s => s.IsOnline)
            .OrderByDescending(s => s.LoginAt)
            .ToListAsync();
    }

    public void Update(Session session)
    {
        _context.Sessions.Update(session);
    }

    public async Task SaveChangesAsync()
    {

        await _context.SaveChangesAsync();
    }
}