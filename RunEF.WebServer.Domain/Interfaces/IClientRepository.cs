using RunEF.WebServer.Domain.Entities;

namespace RunEF.WebServer.Domain.Interfaces;

public interface IClientRepository : IRepository<RunEFClient>
{
    Task<RunEFClient?> GetByComputerCodeAsync(string computerCode, CancellationToken cancellationToken = default);
    Task<IEnumerable<RunEFClient>> GetOnlineClientsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<RunEFClient>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task UpdateHeartbeatAsync(string computerCode, CancellationToken cancellationToken = default);
}