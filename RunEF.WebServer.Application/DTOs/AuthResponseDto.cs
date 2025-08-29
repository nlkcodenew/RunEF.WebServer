namespace RunEF.WebServer.Application.DTOs;

public record AuthResponseDto
{
    public required string AccessToken { get; init; }
    public required string RefreshToken { get; init; }
    public required DateTime ExpiresAt { get; init; }
    public required string Username { get; init; }
    public required int UserId { get; init; }
    public required string Role { get; init; }
}