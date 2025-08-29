using RunEF.WebServer.Domain.Common;

namespace RunEF.WebServer.Domain.Entities;

public class ApplicationLog : BaseEntity
{
    public required string Username { get; set; }
    public required string Action { get; set; }
    public string? Details { get; set; }
    public string? IpAddress { get; set; }
    public string? ComputerCode { get; set; }
    public DateTime LogTime { get; set; } = DateTime.UtcNow;
    public string? Result { get; set; }

    [System.Diagnostics.CodeAnalysis.SetsRequiredMembers]
    public ApplicationLog(string username, string action)
    {
        Username = username;
        Action = action;
    }

    [System.Diagnostics.CodeAnalysis.SetsRequiredMembers]
    public ApplicationLog(string username, string action, string? details, string? ipAddress, string? computerCode, string? result)
    {
        Username = username;
        Action = action;
        Details = details;
        IpAddress = ipAddress;
        ComputerCode = computerCode;
        Result = result;
    }

    public ApplicationLog() { }
}