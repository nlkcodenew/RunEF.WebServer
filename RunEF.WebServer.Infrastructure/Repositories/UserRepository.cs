using Microsoft.EntityFrameworkCore;
using RunEF.WebServer.Domain.Entities;
using RunEF.WebServer.Domain.Interfaces;
using RunEF.WebServer.Infrastructure.Data;

namespace RunEF.WebServer.Infrastructure.Repositories;

public class UserRepository : BaseRepository<User>, IUserRepository
{
    public UserRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.Username == username && !u.IsDeleted, cancellationToken);
    }

    public async Task<User?> GetByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.RefreshToken == refreshToken && !u.IsDeleted, cancellationToken);
    }

    public async Task<bool> IsUsernameExistsAsync(string username, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(u => u.Username == username && !u.IsDeleted, cancellationToken);
    }

    public async Task<IEnumerable<User>> GetActiveUsersAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.Where(u => u.IsActive && !u.IsDeleted).ToListAsync(cancellationToken);
    }
    
    public async Task<User?> ValidateUserCredentialsAsync(string username, string passwordHash, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(
            u => u.Username == username && 
                 u.PasswordHash == passwordHash && 
                 u.IsActive && 
                 !u.IsDeleted, 
            cancellationToken);
    }
    
    public new async Task UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        user.UpdatedAt = DateTime.UtcNow;
        _dbSet.Update(user);
        await _context.SaveChangesAsync(cancellationToken);
    }
}