
using TaskManager.Domain.Entities;

namespace TaskManager.Application.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByIdAsync(Guid id);
    Task<User?> GetByRefreshTokenAsync(string refreshToken);
    Task AddAsync(User user);
    void Update(User user);
    Task SaveChangesAsync();
    Task<bool> AnyAdminExistsAsync();

    Task<(IEnumerable<User> Items, int Total)> GetPagedAsync(int page, int pageSize, string? search);
    Task<IEnumerable<User>> GetAllActiveAsync(); 



}