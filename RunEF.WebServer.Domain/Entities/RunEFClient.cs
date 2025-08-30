using RunEF.WebServer.Domain.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace RunEF.WebServer.Domain.Entities;

[Table("datarunefRunEFClients")]
public class RunEFClient : BaseEntity
{
    public required string ComputerCode { get; set; }
    public required string IpAddress { get; set; }
    public string? ComputerName { get; set; }
    public string? Username { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsOnline { get; set; } = false;
    public DateTime? LastSeen { get; set; }
    public DateTime? LastHeartbeat { get; set; }
    public string? Version { get; set; }
    public string? Status { get; set; }
    public string? Notes { get; set; }
    public bool IsBlocked { get; set; } = false;
    public DateTime? BlockedAt { get; set; }
    public string? BlockedReason { get; set; }
    public string? BlockedBy { get; set; }

    [System.Diagnostics.CodeAnalysis.SetsRequiredMembers]
    public RunEFClient(string computerCode, string ipAddress)
    {
        ComputerCode = computerCode;
        IpAddress = ipAddress;
        LastSeen = DateTime.UtcNow;
    }

    public RunEFClient() { }

    public void UpdateHeartbeat()
    {
        LastHeartbeat = DateTime.UtcNow;
        LastSeen = DateTime.UtcNow;
        IsOnline = true;
    }
    
    public void UpdateHeartbeat(string ipAddress)
    {
        IpAddress = ipAddress;
        LastHeartbeat = DateTime.UtcNow;
        LastSeen = DateTime.UtcNow;
        IsOnline = true;
    }

    public void SetOffline()
    {
        IsOnline = false;
        LastSeen = DateTime.UtcNow;
    }

    public void Block(string reason, string blockedBy)
    {
        IsBlocked = true;
        BlockedAt = DateTime.UtcNow;
        BlockedReason = reason;
        BlockedBy = blockedBy;
        SetOffline();
    }

    public void Unblock()
    {
        IsBlocked = false;
        BlockedAt = null;
        BlockedReason = null;
        BlockedBy = null;
    }

    public bool IsActiveAndOnline() => IsActive && IsOnline && !IsBlocked;
}