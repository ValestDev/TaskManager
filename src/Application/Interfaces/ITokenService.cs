using TaskManager.Domain.Entities;

namespace TaskManager.Application.Interfaces;

public interface ITokenService
{
    string GenerateAccessToken(User user, out DateTime expiresAt);
    string GenerateRefreshToken();
}