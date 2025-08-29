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
    }
}