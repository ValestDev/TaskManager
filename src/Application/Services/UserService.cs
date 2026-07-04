using TaskManager.Application.DTOs;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Enums;

namespace TaskManager.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IAuditService _auditService;

    public UserService(IUserRepository userRepository, IAuditService auditService)
    {
        _userRepository = userRepository;
        _auditService = auditService;
    }

    public async Task<PagedResultDto<UserDto>> GetPagedAsync(int page, int pageSize, string? search)
    {
        var (items, total) = await _userRepository.GetPagedAsync(page, pageSize, search);

        return new PagedResultDto<UserDto>
        {
            Items = items.Select(MapToDto),
            Total = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<UserDto> GetByIdAsync(Guid id)
    {
        var user = await _userRepository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("Usuario no encontrado.");

        return MapToDto(user);
    }

    public async Task<UserDto> CreateAsync(CreateUserDto dto)
    {
        var existing = await _userRepository.GetByEmailAsync(dto.Email);
        if (existing is not null)
        {
            throw new InvalidOperationException("Ya existe un usuario con ese correo electrónico.");
        }

        var user = new User
        {
            Name = dto.Name,
            Email = dto.Email.Trim().ToLower(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role = dto.Role,
            IsActive = true
        };

        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();

        await _auditService.LogAsync(AuditAction.UserCreated, user.Id, user.Email, "Usuario creado");

        return MapToDto(user);
    }

    public async Task<UserDto> UpdateAsync(Guid id, UpdateUserDto dto)
    {
        var user = await _userRepository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("Usuario no encontrado.");

        var existingWithEmail = await _userRepository.GetByEmailAsync(dto.Email);
        if (existingWithEmail is not null && existingWithEmail.Id != id)
        {
            throw new InvalidOperationException("Ya existe otro usuario con ese correo electrónico.");
        }

        user.Name = dto.Name;
        user.Email = dto.Email.Trim().ToLower();
        user.Role = dto.Role;

        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync();

        await _auditService.LogAsync(AuditAction.UserUpdated, user.Id, user.Email, "Usuario actualizado");

        return MapToDto(user);
    }

    public async Task DeactivateAsync(Guid id)
    {
        var user = await _userRepository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("Usuario no encontrado.");

        user.IsActive = false;
        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync();

        await _auditService.LogAsync(AuditAction.UserDeactivated, user.Id, user.Email, "Usuario desactivado");
    }

    public async Task ReactivateAsync(Guid id)
    {
        var user = await _userRepository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("Usuario no encontrado.");

        user.IsActive = true;
        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync();

        await _auditService.LogAsync(AuditAction.UserReactivated, user.Id, user.Email, "Usuario reactivado");
    }

    public async Task<IEnumerable<UserDto>> GetAllActiveAsync()
    {
        var users = await _userRepository.GetAllActiveAsync();
        return users.Select(MapToDto);
    }

    private static UserDto MapToDto(User user) => new()
    {
        Id = user.Id,
        Name = user.Name,
        Email = user.Email,
        Role = user.Role,
        IsActive = user.IsActive,
        CreatedAt = user.CreatedAt
    };
}