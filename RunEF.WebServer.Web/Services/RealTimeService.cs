using Microsoft.AspNetCore.SignalR;
using RunEF.WebServer.Web.Hubs;
using RunEF.WebServer.Web.Models;

namespace RunEF.WebServer.Web.Services
{
    public interface IRealTimeService
    {
        Task SendClientUpdate(ClientModel client);
        Task SendClientStatusChange(int clientId, bool isOnline);
        Task SendSystemLog(string message, string level = "Info");
        Task SendDashboardUpdate(DashboardViewModel dashboard);
        Task SendClientStatusUpdate(int clientId, bool isOnline, bool isBlocked = false);
        Task SendFactoryStatusUpdate(int clientId, string factory, string status);
        Task SendCommandResponse(int clientId, string command, string result);
    }

    public class RealTimeService : IRealTimeService
    {
        private readonly IHubContext<MonitoringHub> _hubContext;
        private readonly ILogger<RealTimeService> _logger;

        public RealTimeService(IHubContext<MonitoringHub> hubContext, ILogger<RealTimeService> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task SendClientUpdate(ClientModel client)
        {
            try
            {
                await _hubContext.Clients.Group("Monitoring").SendAsync("ClientUpdated", client);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending client update");
            }
        }

        public async Task SendClientStatusChange(int clientId, bool isOnline)
        {
            try
            {
                await _hubContext.Clients.Group("Monitoring").SendAsync("ClientStatusChanged", new { ClientId = clientId, IsOnline = isOnline, Timestamp = DateTime.Now });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending client status change");
            }
        }

        public async Task SendSystemLog(string message, string level = "Info")
        {
            try
            {
                await _hubContext.Clients.Group("Monitoring").SendAsync("SystemLog", new { Message = message, Level = level, Timestamp = DateTime.Now });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending system log");
            }
        }

        public async Task SendDashboardUpdate(DashboardViewModel dashboard)
        {
            try
            {
                await _hubContext.Clients.Group("Monitoring").SendAsync("DashboardUpdated", dashboard);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending dashboard update");
            }
        }
        
        public async Task SendClientStatusUpdate(int clientId, bool isOnline, bool isBlocked = false)
        {
            try
            {
                await _hubContext.Clients.Group("Monitoring").SendAsync("ClientStatusUpdated", clientId, new
                {
                    isOnline = isOnline,
                    isBlocked = isBlocked,
                    lastUpdated = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending client status update for client {ClientId}", clientId);
            }
        }
        
        public async Task SendFactoryStatusUpdate(int clientId, string factory, string status)
        {
            try
            {
                await _hubContext.Clients.Group("Monitoring").SendAsync("FactoryStatusUpdated", clientId, factory, status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending factory status update for client {ClientId}, factory {Factory}", clientId, factory);
            }
        }
        
        public async Task SendCommandResponse(int clientId, string command, string result)
        {
            try
            {
                await _hubContext.Clients.Group("Monitoring").SendAsync("CommandResponse", clientId, command, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending command response for client {ClientId}", clientId);
            }
        }
    }
}