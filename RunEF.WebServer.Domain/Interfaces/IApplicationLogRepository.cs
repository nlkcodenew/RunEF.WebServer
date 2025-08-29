using RunEF.WebServer.Domain.Entities;

namespace RunEF.WebServer.Domain.Interfaces;

public interface IApplicationLogRepository : IRepository<ApplicationLog>
{
    Task<IEnumerable<ApplicationLog>> GetByComputerCodeAsync(string computerCode, CancellationToken cancellationToken = default);
    Task<IEnumerable<ApplicationLog>> GetByIpAddressAsync(string ipAddress, CancellationToken cancellationToken = default);
    Task<IEnumerable<ApplicationLog>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<IEnumerable<ApplicationLog>> GetByActivityTypeAsync(string activityType, CancellationToken cancellationToken = default);
    Task<IEnumerable<ApplicationLog>> GetRecentActivitiesAsync(int count = 100, CancellationToken cancellationToken = default);
}