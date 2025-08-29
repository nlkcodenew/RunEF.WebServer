using RunEF.WebServer.Application.Common;
using RunEF.WebServer.Application.DTOs;

namespace RunEF.WebServer.Application.Interfaces;

public interface IClientService
{
    Task<Result<IEnumerable<ClientDto>>> GetAllClientsAsync(CancellationToken cancellationToken = default);
    Task<Result<ClientDto>> GetClientByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<ClientDto>> GetClientByComputerCodeAsync(string computerCode, CancellationToken cancellationToken = default);
    Task<Result<ClientDto>> CreateClientAsync(CreateClientRequest request, CancellationToken cancellationToken = default);
    Task<Result<ClientDto>> UpdateClientAsync(Guid id, UpdateClientRequest request, CancellationToken cancellationToken = default);
    Task<Result<bool>> DeleteClientAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<bool>> BlockClientAsync(Guid id, BlockClientRequest request, CancellationToken cancellationToken = default);
    Task<Result<bool>> UnblockClientAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<bool>> UpdateHeartbeatAsync(string computerCode, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<ClientDto>>> GetOnlineClientsAsync(CancellationToken cancellationToken = default);
}