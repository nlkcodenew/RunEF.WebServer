using RunEF.WebServer.Domain.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace RunEF.WebServer.Domain.Entities;

[Table("datarunefApplicationLogs")]
public class ApplicationLog : BaseEntity
{
    public string? Username { get; set; }
    public string? Action { get; set; }
    public string? Details { get; set; }
    public string? IpAddress { get; set; }
    public string? ComputerCode { get; set; }
    public DateTime LogTime { get; set; } = DateTime.UtcNow;
    public string? Result { get; set; }
    public string? LogType { get; set; }
    public string? Message { get; set; }
    public new DateTime CreatedAt { get; set; } = DateTime.UtcNow;

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