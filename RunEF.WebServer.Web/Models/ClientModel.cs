namespace RunEF.WebServer.Web.Models;

public class ClientModel
{
    public int Id { get; set; }
    public string ComputerCode { get; set; } = string.Empty;
    public string? IpAddress { get; set; }
    public string? ComputerName { get; set; }
    public string Username { get; set; } = string.Empty;
    public DateTime? LastHeartbeat { get; set; }
    public DateTime? LastSeen { get; set; }
    public string? Version { get; set; }
    public string? Status { get; set; }
    public bool IsBlocked { get; set; }
    public string? BlockedReason { get; set; }
    public string? BlockedBy { get; set; }
    public DateTime? BlockedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    public bool IsOnline => LastHeartbeat.HasValue && 
                           LastHeartbeat.Value > DateTime.UtcNow.AddMinutes(-5);
}