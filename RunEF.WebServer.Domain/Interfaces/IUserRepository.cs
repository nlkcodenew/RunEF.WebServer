using RunEF.WebServer.Domain.Entities;

namespace RunEF.WebServer.Domain.Interfaces;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task<User?> GetByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
    Task<bool> IsUsernameExistsAsync(string username, CancellationToken cancellationToken = default);
    Task<User?> ValidateUserCredentialsAsync(string username, string passwordHash, CancellationToken cancellationToken = default);
    new Task UpdateAsync(User user, CancellationToken cancellationToken = default);
}