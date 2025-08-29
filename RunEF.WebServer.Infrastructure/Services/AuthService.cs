using RunEF.WebServer.Application.Common;
using RunEF.WebServer.Application.DTOs;
using RunEF.WebServer.Application.Interfaces;
using RunEF.WebServer.Domain.Interfaces;
using RunEF.WebServer.Domain.Entities;
using RunEF.WebServer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;

namespace RunEF.WebServer.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _context;
    private readonly IJwtService _jwtService;
    private readonly IApplicationLogRepository _logRepository;

    public AuthService(
        ApplicationDbContext context,
        IJwtService jwtService,
        IApplicationLogRepository logRepository)
    {
        _context = context;
        _jwtService = jwtService;
        _logRepository = logRepository;
    }

    public async Task<Result<LoginResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _context.DataRunEFAccountWebs
            .FirstOrDefaultAsync(u => u.Username == request.Username && !u.IsDeleted, cancellationToken);
            
        if (user == null || !user.IsActive)
        {
            return Result<LoginResponse>.Failure("Invalid credentials or account is inactive");
        }

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return Result<LoginResponse>.Failure("Invalid credentials");
        }

        // Update user login info
        user.LastLoginAt = DateTime.UtcNow;
        user.IpAddress = request.IpAddress;
        user.ComputerCode = request.ComputerCode;

        // Generate tokens
        var accessToken = _jwtService.GenerateAccessToken(user);
        var refreshToken = _jwtService.GenerateRefreshToken();
        var expiresAt = _jwtService.GetTokenExpiration(accessToken);

        // Update refresh token
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

        await _context.SaveChangesAsync(cancellationToken);

        // Log activity
        var log = new ApplicationLog(
            user.Username,
            "Login",
            $"User {user.Username} logged in successfully",
            request.IpAddress ?? "Unknown",
            request.ComputerCode ?? "Unknown",
            "Success");

        await _logRepository.AddAsync(log, cancellationToken);

        var response = new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = expiresAt ?? DateTime.UtcNow.AddMinutes(60),
            Username = user.Username,
            UserId = user.Id,
            Role = user.Role
        };

        return Result<LoginResponse>.Success(response);
    }

    public async Task<Result<bool>> LogoutAsync(int userId, CancellationToken cancellationToken = default)
    {
        // TODO: Implement logout for DataRunEFAccountWeb with int Id
        // This method needs to be updated to work with DataRunEFAccountWeb entity
        return Result<bool>.Success(true);
    }

    public async Task<Result<LoginResponse>> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var user = await _context.DataRunEFAccountWebs
            .FirstOrDefaultAsync(u => u.RefreshToken == refreshToken && !u.IsDeleted, cancellationToken);
            
        if (user == null || user.RefreshTokenExpiryTime == null || user.RefreshTokenExpiryTime.Value <= DateTime.UtcNow)
        {
            return Result<LoginResponse>.Failure("Invalid or expired refresh token");
        }

        // Generate new tokens
        var accessToken = _jwtService.GenerateAccessToken(user);
        var newRefreshToken = _jwtService.GenerateRefreshToken();
        var expiresAt = _jwtService.GetTokenExpiration(accessToken);

        // Update refresh token
        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        await _context.SaveChangesAsync(cancellationToken);

        var response = new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = newRefreshToken,
            ExpiresAt = expiresAt ?? DateTime.UtcNow.AddMinutes(60),
            Username = user.Username,
            UserId = user.Id,
            Role = user.Role
        };

        return Result<LoginResponse>.Success(response);
    }

    public Task<Result<bool>> ValidateTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        try
        {
            var isValid = _jwtService.ValidateToken(token);
            return Task.FromResult(Result<bool>.Success(isValid));
        }
        catch (Exception ex)
        {
            return Task.FromResult(Result<bool>.Failure($"Token validation failed: {ex.Message}"));
        }
    }
}