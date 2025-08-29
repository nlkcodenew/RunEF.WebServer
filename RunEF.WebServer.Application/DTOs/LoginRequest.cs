using System.ComponentModel.DataAnnotations;

namespace RunEF.WebServer.Application.DTOs;

public record LoginRequest
{
    [Required(ErrorMessage = "Username is required")]
    public required string Username { get; init; }

    [Required(ErrorMessage = "Password is required")]
    public required string Password { get; init; }

    public string? ComputerCode { get; init; }
    public string? IpAddress { get; init; }
    public string? RemoteIp { get; init; }
}