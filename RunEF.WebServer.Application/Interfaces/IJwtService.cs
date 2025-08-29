using RunEF.WebServer.Domain.Entities;
using System.Security.Claims;

namespace RunEF.WebServer.Application.Interfaces;

public interface IJwtService
{
    string GenerateAccessToken(User user);
    string GenerateAccessToken(DataRunEFAccountWeb user);
    string GenerateRefreshToken();
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
    bool ValidateToken(string token);
    Guid? GetUserIdFromToken(string token);
    DateTime? GetTokenExpiration(string token);
    bool IsTokenExpired(string token);
}