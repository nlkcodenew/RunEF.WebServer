using RunEF.WebServer.Application.Common;
using RunEF.WebServer.Application.DTOs;

namespace RunEF.WebServer.Application.Interfaces;

public interface IAuthService
{
    Task<Result<LoginResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task<Result<bool>> LogoutAsync(int userId, CancellationToken cancellationToken = default);
    Task<Result<LoginResponse>> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
    Task<Result<bool>> ValidateTokenAsync(string token, CancellationToken cancellationToken = default);
}