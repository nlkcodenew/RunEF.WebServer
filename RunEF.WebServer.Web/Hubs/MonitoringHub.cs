using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;

namespace RunEF.WebServer.Web.Hubs
{
    [Authorize]
    public class MonitoringHub : Hub
    {
        public async Task JoinGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        }

        public async Task LeaveGroup(string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        }

        public override async Task OnConnectedAsync()
        {
            // Add user to monitoring group
            await Groups.AddToGroupAsync(Context.ConnectionId, "Monitoring");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "Monitoring");
            await base.OnDisconnectedAsync(exception);
        }
    }
}