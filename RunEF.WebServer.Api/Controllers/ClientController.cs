using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RunEF.WebServer.Infrastructure.Data;
using RunEF.WebServer.Domain.Entities;
using System.Text.Json;
using System.Data;

namespace RunEF.WebServer.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClientController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ClientController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpPost("status")]
    public async Task<ActionResult> UpdateStatus([FromBody] ClientStatusRequest request)
    {
        try
        {
            var computerCode = HttpContext.Items["ComputerCode"]?.ToString();
            var clientIP = HttpContext.Items["ClientIP"]?.ToString();

            if (string.IsNullOrEmpty(computerCode))
            {
                return BadRequest("Computer code not found");
            }

            // Tìm hoặc tạo client
            var client = await _context.RunEFClients
                .FirstOrDefaultAsync(c => c.ComputerCode == computerCode);

            if (client == null)
            {
                client = new RunEFClient
                {
                    ComputerCode = computerCode,
                    IpAddress = clientIP ?? request.IpAddressClient,
                    ComputerName = Environment.MachineName,
                    Username = Environment.UserName,
                    IsActive = true,
                    IsOnline = true,
                    LastSeen = DateTime.UtcNow,
                    LastHeartbeat = DateTime.UtcNow,
                    Status = request.StatusClient
                };
                _context.RunEFClients.Add(client);
            }
            else
            {
                client.IpAddress = clientIP ?? request.IpAddressClient;
                client.IsOnline = true;
                client.LastSeen = DateTime.UtcNow;
                client.LastHeartbeat = DateTime.UtcNow;
                client.Status = request.StatusClient;
                _context.RunEFClients.Update(client);
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = "Status updated successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("restart-command/{ipAddress}")]
    public async Task<ActionResult> GetRestartCommand(string ipAddress)
    {
        try
        {
            var computerCode = HttpContext.Items["ComputerCode"]?.ToString();
            
            if (string.IsNullOrEmpty(computerCode))
            {
                return BadRequest("Computer code not found");
            }

            // Kiểm tra xem có lệnh restart nào cho client này không
            var client = await _context.RunEFClients
                .FirstOrDefaultAsync(c => c.ComputerCode == computerCode);

            if (client == null)
            {
                return NotFound("Client not found");
            }

            // Tạm thời trả về "No" - có thể mở rộng để kiểm tra bảng restart commands
            return Ok(new { command = "No" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("restart-command")]
    public async Task<ActionResult> UpdateRestartCommand([FromBody] RestartCommandRequest request)
    {
        try
        {
            var computerCode = HttpContext.Items["ComputerCode"]?.ToString();
            
            if (string.IsNullOrEmpty(computerCode))
            {
                return BadRequest("Computer code not found");
            }

            // Cập nhật trạng thái restart command
            // Có thể lưu vào bảng riêng hoặc cập nhật client status
            var client = await _context.RunEFClients
                .FirstOrDefaultAsync(c => c.ComputerCode == computerCode);

            if (client != null)
            {
                client.Status = $"Restart: {request.Status}";
                client.LastSeen = DateTime.UtcNow;
                _context.RunEFClients.Update(client);
                await _context.SaveChangesAsync();
            }

            return Ok(new { message = "Restart command status updated" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("folders")]
    public async Task<ActionResult> SendFolderData([FromBody] object folderData)
    {
        try
        {
            var computerCode = HttpContext.Items["ComputerCode"]?.ToString();
            
            if (string.IsNullOrEmpty(computerCode))
            {
                return BadRequest("Computer code not found");
            }

            // Lưu folder data vào bảng trung tâm
            await SaveToCentralLogsTable(folderData, computerCode, "API_SERVER", "FolderData");

            return Ok(new { message = "Folder data received successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("activity-log")]
    public async Task<ActionResult> SendActivityLog([FromBody] object logData)
    {
        try
        {
            var computerCode = HttpContext.Items["ComputerCode"]?.ToString();
            
            if (string.IsNullOrEmpty(computerCode))
            {
                return BadRequest("Computer code not found");
            }

            // Lưu vào bảng trung tâm datarunefLogs
            await SaveToCentralLogsTable(logData, computerCode, "CLIENT");

            return Ok(new { message = "Activity log received successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    private async Task SaveToCentralLogsTable(object logData, string computerCode, string logSource, string activityType = null)
    {
        try
        {
            // Parse log data từ JSON
            var logJson = JsonSerializer.Serialize(logData);
            var logDict = JsonSerializer.Deserialize<Dictionary<string, object>>(logJson);

            // Sử dụng stored procedure để insert vào bảng trung tâm
            var sql = @"EXEC datarunef_sp_AddLog 
                        @log_source, @log_level, @username, @computer_code, @computer_name, @remote_ip,
                        @session_id, @session_start_time, @session_end_time,
                        @activity_category, @activity_type, @activity_action,
                        @target_type, @target_path, @target_name, @process_name,
                        @activity_details, @result_status, @error_message,
                        @client_version, @api_version, @additional_data";

            using var connection = _context.Database.GetDbConnection();
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = sql;
            command.CommandType = System.Data.CommandType.Text;

            // Add parameters cho stored procedure
            AddParameter(command, "@log_source", logSource);
            AddParameter(command, "@log_level", GetValue(logDict, "resultStatus") == "FAILED" ? "ERROR" : "INFO");
            AddParameter(command, "@username", GetValue(logDict, "username") ?? Environment.UserName);
            AddParameter(command, "@computer_code", computerCode);
            AddParameter(command, "@computer_name", GetValue(logDict, "computerName"));
            AddParameter(command, "@remote_ip", GetValue(logDict, "remoteIP"));
            AddParameter(command, "@session_id", GetValue(logDict, "sessionId"));
            AddParameter(command, "@session_start_time", null); // Có thể mở rộng sau
            AddParameter(command, "@session_end_time", null);   // Có thể mở rộng sau
            AddParameter(command, "@activity_category", GetValue(logDict, "activityCategory") ?? "APPLICATION");
            AddParameter(command, "@activity_type", activityType ?? GetValue(logDict, "activityType") ?? "GENERAL");
            AddParameter(command, "@activity_action", GetValue(logDict, "activityAction"));
            AddParameter(command, "@target_type", DetermineTargetType(GetValue(logDict, "targetPath")));
            AddParameter(command, "@target_path", GetValue(logDict, "targetPath"));
            AddParameter(command, "@target_name", GetValue(logDict, "targetName"));
            AddParameter(command, "@process_name", GetValue(logDict, "processName"));
            AddParameter(command, "@activity_details", GetValue(logDict, "activityDetails"));
            AddParameter(command, "@result_status", GetValue(logDict, "resultStatus") ?? "SUCCESS");
            AddParameter(command, "@error_message", GetValue(logDict, "errorMessage"));
            AddParameter(command, "@client_version", GetValue(logDict, "clientVersion"));
            AddParameter(command, "@api_version", "1.0");
            AddParameter(command, "@additional_data", logJson);

            await command.ExecuteNonQueryAsync();
        }
        catch (Exception ex)
        {
            // Log error nhưng không throw để không ảnh hưởng đến main flow
            Console.WriteLine($"Error saving to central logs table: {ex.Message}");
        }
    }

    private string DetermineTargetType(string targetPath)
    {
        if (string.IsNullOrEmpty(targetPath)) return null;
        
        if (targetPath.Contains("\\") || targetPath.Contains("/"))
        {
            return System.IO.Path.HasExtension(targetPath) ? "FILE" : "FOLDER";
        }
        
        return "PROCESS";
    }

    private void AddParameter(System.Data.Common.DbCommand command, string name, object value)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = name;
        parameter.Value = value ?? DBNull.Value;
        command.Parameters.Add(parameter);
    }

    private string GetValue(Dictionary<string, object> dict, string key)
    {
        return dict.ContainsKey(key) && dict[key] != null ? dict[key].ToString() : null;
    }

    [HttpGet("folder-restart/{ipAddress}")]
    public async Task<ActionResult> GetFolderRestart(string ipAddress)
    {
        try
        {
            var computerCode = HttpContext.Items["ComputerCode"]?.ToString();
            
            if (string.IsNullOrEmpty(computerCode))
            {
                return BadRequest("Computer code not found");
            }

            // Kiểm tra client tồn tại
            var client = await _context.RunEFClients
                .FirstOrDefaultAsync(c => c.ComputerCode == computerCode);

            if (client == null)
            {
                return NotFound("Client not found");
            }

            // Trả về danh sách folder cần restart
            // Tạm thời trả về list rỗng
            return Ok(new List<object>());
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("folder-restart")]
    public async Task<ActionResult> UpdateFolderRestartStatus([FromBody] FolderRestartRequest request)
    {
        try
        {
            var computerCode = HttpContext.Items["ComputerCode"]?.ToString();
            
            if (string.IsNullOrEmpty(computerCode))
            {
                return BadRequest("Computer code not found");
            }

            // Cập nhật trạng thái folder restart vào bảng trung tâm
            await SaveToCentralLogsTable(request, computerCode, "API_SERVER", "FolderRestart");

            return Ok(new { message = "Folder restart status updated" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

// Request Models
public class ClientStatusRequest
{
    public string IpAddressClient { get; set; } = string.Empty;
    public string StatusClient { get; set; } = string.Empty;
    public int EfCount { get; set; }
    public int RunningEFCount { get; set; }
    public string PauseStatus { get; set; } = string.Empty;
    public DateTime DateTimeAdd { get; set; }
}

public class RestartCommandRequest
{
    public string IpAddressClient { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}

public class FolderRestartRequest
{
    public string FolderName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string IpAddressClient { get; set; } = string.Empty;
}
