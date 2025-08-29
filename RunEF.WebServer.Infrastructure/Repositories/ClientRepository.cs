using Microsoft.EntityFrameworkCore;
using RunEF.WebServer.Domain.Entities;
using RunEF.WebServer.Domain.Interfaces;
using RunEF.WebServer.Infrastructure.Data;

namespace RunEF.WebServer.Infrastructure.Repositories;

public class ClientRepository : BaseRepository<RunEFClient>, IClientRepository
{
    public ClientRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<RunEFClient?> GetByComputerCodeAsync(string computerCode, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(c => c.ComputerCode == computerCode && !c.IsDeleted, cancellationToken);
    }

    public async Task<IEnumerable<RunEFClient>> GetOnlineClientsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.Where(c => c.IsOnline && !c.IsDeleted).ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<RunEFClient>> GetClientsByIpAddressAsync(string ipAddress, CancellationToken cancellationToken = default)
    {
        return await _dbSet.Where(c => c.IpAddress == ipAddress && !c.IsDeleted).ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<RunEFClient>> GetInactiveClientsAsync(TimeSpan inactiveThreshold, CancellationToken cancellationToken = default)
    {
        var cutoffTime = DateTime.UtcNow.Subtract(inactiveThreshold);
        return await _dbSet.Where(c => c.LastSeen < cutoffTime && !c.IsDeleted).ToListAsync(cancellationToken);
    }
    
    public async Task<IEnumerable<RunEFClient>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(c => c.CreatedAt >= startDate && c.CreatedAt <= endDate && !c.IsDeleted)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);
    }
    
    public async Task UpdateHeartbeatAsync(string computerCode, CancellationToken cancellationToken = default)
    {
        var client = await GetByComputerCodeAsync(computerCode, cancellationToken);
        if (client != null)
        {
            client.LastSeen = DateTime.UtcNow;
            client.IsOnline = true;
            await UpdateAsync(client, cancellationToken);
        }
    }
}