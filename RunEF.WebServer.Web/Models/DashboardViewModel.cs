namespace RunEF.WebServer.Web.Models;

public class DashboardViewModel
{
    public int TotalClients { get; set; }
    public int OnlineClientsCount { get; set; }
    public int OfflineClientsCount { get; set; }
    public List<ClientModel> AllClients { get; set; } = new();
    public List<ClientModel> OnlineClients { get; set; } = new();
}