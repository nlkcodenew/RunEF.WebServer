using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using RunEF.WebServer.Domain.Entities;
using RunEF.WebServer.Domain.Interfaces;
using RunEF.WebServer.Infrastructure.Data;
using RunEF.WebServer.Application.Common;

namespace RunEF.WebServer.Infrastructure.Repositories;

public class ApplicationLogRepository : BaseRepository<ApplicationLog>, IApplicationLogRepository
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<ApplicationLogRepository> _logger;

    public ApplicationLogRepository(
        ApplicationDbContext context, 
        IMemoryCache cache, 
        ILogger<ApplicationLogRepository> logger) : base(context)
    {
        _cache = cache;
        _logger = logger;
    }
    
    public async Task<IEnumerable<ApplicationLog>> GetByIpAddressAsync(string ipAddress, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(log => log.IpAddress == ipAddress && !log.IsDeleted)
            .OrderByDescending(log => log.LogTime)
            .Take(100)
            .ToListAsync(cancellationToken);
    }
    
    public async Task<IEnumerable<ApplicationLog>> GetByActivityTypeAsync(string activityType, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(log => log.Action == activityType && !log.IsDeleted)
            .OrderByDescending(log => log.LogTime)
            .Take(100)
            .ToListAsync(cancellationToken);
    }
    
    public async Task<IEnumerable<ApplicationLog>> GetRecentActivitiesAsync(int count = 100, CancellationToken cancellationToken = default)
    {
        return await GetRecentLogsAsync(count, cancellationToken);
    }

    public async Task<IEnumerable<ApplicationLog>> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        try
        {
            var cacheKey = $"user_logs_{username}";
            
            if (_cache.TryGetValue(cacheKey, out IEnumerable<ApplicationLog>? cached))
            {
                _logger.LogInformation("Cache hit for user logs: {Username}", username);
                return cached ?? Enumerable.Empty<ApplicationLog>();
            }

            var logs = await _dbSet
                .Where(log => log.Username == username && !log.IsDeleted)
                .OrderByDescending(log => log.LogTime)
                .Take(100)
                .ToListAsync(cancellationToken);

            _cache.Set(cacheKey, logs, TimeSpan.FromMinutes(5));

            return logs;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting logs for user: {Username}", username);
            throw;
        }
    }

    public async Task<IEnumerable<ApplicationLog>> GetByComputerCodeAsync(string computerCode, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(log => log.ComputerCode == computerCode && !log.IsDeleted)
            .OrderByDescending(log => log.LogTime)
            .Take(100)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ApplicationLog>> GetByActionAsync(string action, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(log => log.Action == action && !log.IsDeleted)
            .OrderByDescending(log => log.LogTime)
            .Take(100)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ApplicationLog>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(log => log.LogTime >= startDate && log.LogTime <= endDate && !log.IsDeleted)
            .OrderByDescending(log => log.LogTime)
            .Take(1000)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ApplicationLog>> GetRecentLogsAsync(int count = 100, CancellationToken cancellationToken = default)
    {
        try
        {
            var cacheKey = $"recent_logs_{count}";
            
            if (_cache.TryGetValue(cacheKey, out IEnumerable<ApplicationLog>? cached))
            {
                return cached ?? Enumerable.Empty<ApplicationLog>();
            }

            var logs = await _dbSet
                .Where(log => !log.IsDeleted)
                .OrderByDescending(log => log.LogTime)
                .Take(count)
                .ToListAsync(cancellationToken);

            _cache.Set(cacheKey, logs, TimeSpan.FromMinutes(2));

            return logs;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting recent logs: {Count}", count);
            throw;
        }
    }
}