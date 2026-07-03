using TaskManager.Application.DTOs;

namespace TaskManager.Application.Interfaces;

public interface IUserService
{
    Task<PagedResultDto<UserDto>> GetPagedAsync(int page, int pageSize, string? search);
    Task<UserDto> GetByIdAsync(Guid id);
    Task<UserDto> CreateAsync(CreateUserDto dto);
    Task<UserDto> UpdateAsync(Guid id, UpdateUserDto dto);
    Task DeactivateAsync(Guid id);
    Task ReactivateAsync(Guid id);

    Task<IEnumerable<UserDto>> GetAllActiveAsync();
}