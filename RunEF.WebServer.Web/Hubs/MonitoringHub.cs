using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using System.Diagnostics;

namespace RunEF.WebServer.Web.Hubs
{
    [Authorize]
    public class MonitoringHub : Hub
    {
        private readonly ILogger<MonitoringHub> _logger;
        
        public MonitoringHub(ILogger<MonitoringHub> logger)
        {
            _logger = logger;
        }
        
        public async Task JoinGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        }

        public async Task LeaveGroup(string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        }
        
        // Factory Control Methods
        public async Task SendFactoryCommand(string factory, string command)
        {
            try
            {
                _logger.LogInformation($"Factory {factory} {command} command received from {Context.UserIdentifier}");
                
                // Simulate factory command execution
                var success = await ExecuteFactoryCommand(factory, command);
                var message = success ? $"{command} command executed successfully" : $"{command} command failed";
                
                await Clients.Caller.SendAsync("FactoryCommandResponse", new 
                { 
                    Factory = factory, 
                    Command = command, 
                    Success = success, 
                    Message = message 
                });
                
                // Broadcast status update to all monitoring clients
                var status = success ? await GetFactoryStatus(factory) : "Error";
                await Clients.Group("Monitoring").SendAsync("FactoryStatusUpdated", new 
                { 
                    Factory = factory, 
                    Status = status 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error executing factory {factory} {command}");
                await Clients.Caller.SendAsync("FactoryCommandResponse", new 
                { 
                    Factory = factory, 
                    Command = command, 
                    Success = false, 
                    Message = $"Error: {ex.Message}" 
                });
            }
        }
        
        public async Task<string> GetFactoryStatus(string factory)
        {
            try
            {
                // Simulate factory status check
                var status = CheckFactoryStatus(factory);
                return status;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting factory {factory} status");
                return "Error";
            }
        }
        
        // Terminal Information Methods
        public async Task GetTerminalInfo()
        {
            try
            {
                var terminalInfo = await GetSystemTerminalInfo();
                await Clients.Caller.SendAsync("TerminalInfoUpdated", terminalInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting terminal information");
            }
        }
        
        private async Task<bool> ExecuteFactoryCommand(string factory, string command)
        {
            // Simulate command execution with delay
            await Task.Delay(1000);
            
            // For demo purposes, randomly succeed/fail
            var random = new Random();
            return random.Next(1, 11) > 2; // 80% success rate
        }
        
        private string CheckFactoryStatus(string factory)
        {
            // Simulate factory status check
            var statuses = new[] { "Running", "Stopped", "Starting", "Stopping" };
            var random = new Random();
            return statuses[random.Next(statuses.Length)];
        }
        
        private async Task<object> GetSystemTerminalInfo()
        {
            try
            {
                // Get system information
                var processCount = Process.GetProcesses().Length;
                var activeTerminals = GetActiveTerminalCount();
                var systemLoad = GetSystemLoad();
                
                return new
                {
                    activeTerminals = activeTerminals,
                    runningProcesses = processCount,
                    systemLoad = systemLoad
                };
            }
            catch
            {
                return new
                {
                    activeTerminals = 0,
                    runningProcesses = 0,
                    systemLoad = "Unknown"
                };
            }
        }
        
        private int GetActiveTerminalCount()
        {
            try
            {
                // Count cmd.exe and powershell processes
                var terminals = Process.GetProcesses()
                    .Where(p => p.ProcessName.ToLower().Contains("cmd") || 
                               p.ProcessName.ToLower().Contains("powershell") ||
                               p.ProcessName.ToLower().Contains("pwsh"))
                    .Count();
                return terminals;
            }
            catch
            {
                return 0;
            }
        }
        
        private string GetSystemLoad()
        {
            try
            {
                var processCount = Process.GetProcesses().Length;
                if (processCount < 50) return "Low";
                if (processCount < 100) return "Medium";
                return "High";
            }
            catch
            {
                return "Unknown";
            }
        }

        // Log Management Methods
        public async Task SendLogEntry(string level, string source, string message, string category = "General")
        {
            try
            {
                var logEntry = new
                {
                    Id = Guid.NewGuid().ToString(),
                    Timestamp = DateTime.Now,
                    Level = level,
                    Source = source,
                    Message = message,
                    Category = category,
                    UserId = Context.UserIdentifier
                };

                // Broadcast to all monitoring clients
                await Clients.Group("Monitoring").SendAsync("NewLogEntry", logEntry);
                
                _logger.LogInformation($"Log entry broadcasted: {level} - {message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error broadcasting log entry");
            }
        }

        public async Task RequestLogStats()
        {
            try
            {
                // Simulate log statistics
                var stats = new
                {
                    TotalLogs = new Random().Next(1000, 5000),
                    ErrorLogs = new Random().Next(10, 100),
                    WarningLogs = new Random().Next(50, 200),
                    LastUpdated = DateTime.Now
                };

                await Clients.Caller.SendAsync("LogStatsUpdated", stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting log statistics");
            }
        }

        // Client management methods
        public async Task SendClientStatusUpdate(int clientId, bool isOnline, bool isBlocked = false)
        {
            await Clients.Group("Monitoring").SendAsync("ClientStatusUpdated", clientId, new
            {
                isOnline = isOnline,
                isBlocked = isBlocked,
                lastUpdated = DateTime.Now
            });
        }
        
        public async Task SendFactoryStatusUpdate(int clientId, string factory, string status)
        {
            await Clients.Group("Monitoring").SendAsync("FactoryStatusUpdated", clientId, factory, status);
        }
        
        public async Task SendCommandResponse(int clientId, string command, string result)
        {
            await Clients.Group("Monitoring").SendAsync("CommandResponse", clientId, command, result);
        }
        
        public async Task RequestClientStatus(int clientId)
        {
            // Simulate client status check
            var isOnline = new Random().Next(0, 100) > 20; // 80% chance of being online
            var isBlocked = new Random().Next(0, 100) > 90; // 10% chance of being blocked
            
            await SendClientStatusUpdate(clientId, isOnline, isBlocked);
        }
        
        public async Task RequestFactoryStatus(int clientId, string factory)
        {
            // Simulate factory status check
            var statuses = new[] { "Running", "Stopped", "Error", "Maintenance" };
            var status = statuses[new Random().Next(statuses.Length)];
            
            await SendFactoryStatusUpdate(clientId, factory, status);
        }

        public override async Task OnConnectedAsync()
        {
            // Add user to monitoring group
            await Groups.AddToGroupAsync(Context.ConnectionId, "Monitoring");
            _logger.LogInformation($"User {Context.UserIdentifier} connected to monitoring hub");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "Monitoring");
            _logger.LogInformation($"User {Context.UserIdentifier} disconnected from monitoring hub");
            await base.OnDisconnectedAsync(exception);
        }
    }
}