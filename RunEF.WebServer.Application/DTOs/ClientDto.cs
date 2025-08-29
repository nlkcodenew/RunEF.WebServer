using System.ComponentModel.DataAnnotations;

namespace RunEF.WebServer.Application.DTOs;

public record ClientDto
{
    public Guid Id { get; init; }
    public string ComputerCode { get; init; } = string.Empty;
    public string IpAddress { get; init; } = string.Empty;
    public string? ComputerName { get; init; }
    public string? Username { get; init; }
    public bool IsOnline { get; init; }
    public DateTime? LastSeen { get; init; }
    public DateTime? LastHeartbeat { get; init; }
    public string? Version { get; init; }
    public string? Status { get; init; }
    public bool IsBlocked { get; init; }
    public string? BlockedReason { get; init; }
}

public record CreateClientRequest
{
    [Required]
    public required string ComputerCode { get; init; }
    
    [Required]
    public required string IpAddress { get; init; }
    
    public string? ComputerName { get; init; }
    public string? Username { get; init; }
    public string? Version { get; init; }
}

public record UpdateClientRequest
{
    public string? ComputerName { get; init; }
    public string? Username { get; init; }
    public string? Version { get; init; }
    public string? Status { get; init; }
}

public record BlockClientRequest
{
    [Required]
    public required string ComputerCode { get; init; }
    
    [Required]
    public required bool IsBlocked { get; init; }
    
    public string? Reason { get; init; }
}