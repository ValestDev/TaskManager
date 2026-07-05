using TaskManager.Application.DTOs;

namespace TaskManager.Application.Interfaces;

public interface IAuthService
{
    Task<LoginResponseDto> LoginAsync(LoginRequestDto request);
    Task<LoginResponseDto> RefreshTokenAsync(string refreshToken);
    Task LogoutAsync(Guid userId);
    Task EnsureAdminSeededAsync();
    Task<IEnumerable<OnlineUserDto>> GetOnlineUsersAsync();
}