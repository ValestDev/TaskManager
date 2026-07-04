using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using TaskManager.Application.DTOs;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Enums;

namespace TaskManager.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly ISessionRepository _sessionRepository;
    private readonly ITokenService _tokenService;
    private readonly IEmailService _emailService;
    private readonly IPresenceNotifier _presenceNotifier;
    private readonly IConfiguration _config;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUserRepository userRepository,
        ISessionRepository sessionRepository,
        ITokenService tokenService,
        IEmailService emailService,
        IPresenceNotifier presenceNotifier,
        IConfiguration config,
        ILogger<AuthService> logger)
    {
        _userRepository = userRepository;
        _sessionRepository = sessionRepository;
        _tokenService = tokenService;
        _emailService = emailService;
        _presenceNotifier = presenceNotifier;
        _config = config;
        _logger = logger;
    }

    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);

        if (user is null || !user.IsActive)
        {
            throw new UnauthorizedAccessException("Credenciales inválidas.");
        }

        var passwordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
        if (!passwordValid)
        {
            throw new UnauthorizedAccessException("Credenciales inválidas.");
        }

        var accessToken = _tokenService.GenerateAccessToken(user, out var expiresAt);
        var refreshToken = _tokenService.GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7);
        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync();

        var session = new Session
        {
            UserId = user.Id,
            LoginAt = DateTime.UtcNow,
            IsOnline = true
        };
        await _sessionRepository.AddAsync(session);
        await _sessionRepository.SaveChangesAsync();

        await _presenceNotifier.NotifyUserOnline(user.Id, user.Name, session.LoginAt);

        return new LoginResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            AccessTokenExpiresAt = expiresAt,
            UserId = user.Id,
            Name = user.Name,
            Email = user.Email,
            Role = user.Role.ToString()
        };
    }

    public async Task<LoginResponseDto> RefreshTokenAsync(string refreshToken)
    {
        var user = await _userRepository.GetByRefreshTokenAsync(refreshToken);

        if (user is null || user.RefreshTokenExpiresAt is null || user.RefreshTokenExpiresAt < DateTime.UtcNow)
        {
            throw new UnauthorizedAccessException("Refresh token inválido o expirado.");
        }

        var newAccessToken = _tokenService.GenerateAccessToken(user, out var expiresAt);
        var newRefreshToken = _tokenService.GenerateRefreshToken();

        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7);
        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync();

        return new LoginResponseDto
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken,
            AccessTokenExpiresAt = expiresAt,
            UserId = user.Id,
            Name = user.Name,
            Email = user.Email,
            Role = user.Role.ToString()
        };
    }

    public async Task LogoutAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null) return;

        user.RefreshToken = null;
        user.RefreshTokenExpiresAt = null;
        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync();

        var session = await _sessionRepository.GetActiveSessionAsync(userId);
        if (session is not null)
        {
            session.IsOnline = false;
            session.LogoutAt = DateTime.UtcNow;
            _sessionRepository.Update(session);
            await _sessionRepository.SaveChangesAsync();
        }

        await _presenceNotifier.NotifyUserOffline(userId, user.Name);
    }

    public async Task EnsureAdminSeededAsync()
    {
        var adminExists = await _userRepository.AnyAdminExistsAsync();
        if (adminExists)
        {
            _logger.LogInformation("Ya existe un administrador, se omite el seed inicial.");
            return;
        }

        var adminEmail = _config["InitialAdmin:Email"] ?? "admin@empresa.com";
        var adminName = _config["InitialAdmin:Name"] ?? "Administrador";
        var randomPassword = GenerateRandomPassword();

        var admin = new User
        {
            Name = adminName,
            Email = adminEmail,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(randomPassword),
            Role = UserRole.Admin,
            IsActive = true
        };

        await _userRepository.AddAsync(admin);
        await _userRepository.SaveChangesAsync();

        _logger.LogWarning(
            "Usuario administrador inicial creado. Email: {Email} | Password temporal: {Password}",
            adminEmail, randomPassword);

        await _emailService.SendAdminCredentialsAsync(adminEmail, randomPassword);
    }

    private static string GenerateRandomPassword()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789!@#$%";
        var bytes = new byte[12];
        RandomNumberGenerator.Fill(bytes);

        var result = new char[12];
        for (int i = 0; i < 12; i++)
        {
            result[i] = chars[bytes[i] % chars.Length];
        }
        return new string(result);
    }

    public async Task<IEnumerable<OnlineUserDto>> GetOnlineUsersAsync()
    {
    var sessions = await _sessionRepository.GetOnlineSessionsAsync();

    return sessions.Select(s => new OnlineUserDto
    {
        UserId = s.UserId,
        UserName = s.User?.Name ?? string.Empty,
        LoginAt = s.LoginAt
    });
    }
}