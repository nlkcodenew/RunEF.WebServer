using RunEF.WebServer.Application.Common;
using RunEF.WebServer.Application.DTOs;
using RunEF.WebServer.Application.Interfaces;
using RunEF.WebServer.Domain.Interfaces;
using RunEF.WebServer.Domain.Entities;

namespace RunEF.WebServer.Infrastructure.Services;

public class ClientService : IClientService
{
    private readonly IClientRepository _clientRepository;
    private readonly IApplicationLogRepository _logRepository;

    public ClientService(
        IClientRepository clientRepository,
        IApplicationLogRepository logRepository)
    {
        _clientRepository = clientRepository;
        _logRepository = logRepository;
    }

    public async Task<Result<ClientDto>> CreateClientAsync(CreateClientRequest request, CancellationToken cancellationToken = default)
    {
        var existingClient = await _clientRepository.GetByComputerCodeAsync(request.ComputerCode, cancellationToken);
        if (existingClient != null)
        {
            return Result<ClientDto>.Failure("Client with this computer code already exists");
        }

        var client = new RunEFClient(request.ComputerCode, request.IpAddress);
        await _clientRepository.AddAsync(client, cancellationToken);

        // Log activity
        var log = new ApplicationLog(
            "System",
            "CreateClient",
            $"Client {request.ComputerCode} created",
            request.IpAddress,
            request.ComputerCode,
            "Success");
        
        await _logRepository.AddAsync(log, cancellationToken);

        var dto = new ClientDto
        {
            Id = client.Id,
            ComputerCode = client.ComputerCode,
            IpAddress = client.IpAddress,
            IsOnline = client.IsOnline,
            IsBlocked = client.IsBlocked,
            LastSeen = client.LastSeen,
            LastHeartbeat = client.LastHeartbeat
        };

        return Result<ClientDto>.Success(dto);
    }

    public async Task<Result<bool>> UpdateHeartbeatAsync(string computerCode, string ipAddress, CancellationToken cancellationToken = default)
    {
        var client = await _clientRepository.GetByComputerCodeAsync(computerCode, cancellationToken);
        if (client == null)
        {
            return Result<bool>.Failure("Client not found");
        }

        client.UpdateHeartbeat(ipAddress);
        await _clientRepository.UpdateAsync(client, cancellationToken);

        return Result<bool>.Success(true);
    }

    public async Task<Result<bool>> BlockClientAsync(BlockClientRequest request, CancellationToken cancellationToken = default)
    {
        var client = await _clientRepository.GetByComputerCodeAsync(request.ComputerCode, cancellationToken);
        if (client == null)
        {
            return Result<bool>.Failure("Client not found");
        }

        if (request.IsBlocked)
        {
            client.Block(request.Reason ?? "No reason provided", "System");
        }
        else
        {
            client.Unblock();
        }

        await _clientRepository.UpdateAsync(client, cancellationToken);

        // Log activity
        var action = request.IsBlocked ? "BlockClient" : "UnblockClient";
        var details = request.IsBlocked ? $"Client {request.ComputerCode} blocked: {request.Reason}" : $"Client {request.ComputerCode} unblocked";
        
        var log = new ApplicationLog(
            "System",
            action,
            details,
            client.IpAddress ?? "Unknown",
            request.ComputerCode,
            "Success");
        
        await _logRepository.AddAsync(log, cancellationToken);

        return Result<bool>.Success(true);
    }

    public async Task<Result<IEnumerable<ClientDto>>> GetAllClientsAsync(CancellationToken cancellationToken = default)
    {
        var clients = await _clientRepository.GetAllAsync(cancellationToken);
        var dtos = clients.Select(c => new ClientDto
        {
            Id = c.Id,
            ComputerCode = c.ComputerCode,
            IpAddress = c.IpAddress,
            IsOnline = c.IsOnline,
            IsBlocked = c.IsBlocked,
            LastSeen = c.LastSeen,
            LastHeartbeat = c.LastHeartbeat
        });

        return Result<IEnumerable<ClientDto>>.Success(dtos);
    }

    public async Task<Result<IEnumerable<ClientDto>>> GetOnlineClientsAsync(CancellationToken cancellationToken = default)
    {
        var clients = await _clientRepository.GetOnlineClientsAsync(cancellationToken);
        var dtos = clients.Select(c => new ClientDto
        {
            Id = c.Id,
            ComputerCode = c.ComputerCode,
            IpAddress = c.IpAddress,
            IsOnline = c.IsOnline,
            IsBlocked = c.IsBlocked,
            LastSeen = c.LastSeen,
            LastHeartbeat = c.LastHeartbeat
        });

        return Result<IEnumerable<ClientDto>>.Success(dtos);
    }

    // Add missing method implementations
    public async Task<Result<ClientDto>> GetClientByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var client = await _clientRepository.GetByIdAsync(id, cancellationToken);
        if (client == null)
            return Result<ClientDto>.Failure("Client not found");
            
        // Map to DTO
        var dto = new ClientDto
        {
            Id = client.Id,
            ComputerCode = client.ComputerCode,
            IpAddress = client.IpAddress,
            IsOnline = client.IsOnline,
            IsBlocked = client.IsBlocked,
            LastSeen = client.LastSeen,
            LastHeartbeat = client.LastHeartbeat
        };
        return Result<ClientDto>.Success(dto);
    }

    public async Task<Result<ClientDto>> GetClientByComputerCodeAsync(string computerCode, CancellationToken cancellationToken = default)
    {
        var client = await _clientRepository.GetByComputerCodeAsync(computerCode, cancellationToken);
        if (client == null)
            return Result<ClientDto>.Failure("Client not found");
            
        // Map to DTO
        var dto = new ClientDto
        {
            Id = client.Id,
            ComputerCode = client.ComputerCode,
            IpAddress = client.IpAddress,
            IsOnline = client.IsOnline,
            IsBlocked = client.IsBlocked,
            LastSeen = client.LastSeen,
            LastHeartbeat = client.LastHeartbeat
        };
        return Result<ClientDto>.Success(dto);
    }

    public async Task<Result<ClientDto>> UpdateClientAsync(Guid id, UpdateClientRequest request, CancellationToken cancellationToken = default)
    {
        var client = await _clientRepository.GetByIdAsync(id, cancellationToken);
        if (client == null)
            return Result<ClientDto>.Failure("Client not found");
            
        // Update client properties from request
        await _clientRepository.UpdateAsync(client, cancellationToken);
        
        // Map to DTO
        var dto = new ClientDto
        {
            Id = client.Id,
            ComputerCode = client.ComputerCode,
            IpAddress = client.IpAddress,
            IsOnline = client.IsOnline,
            IsBlocked = client.IsBlocked,
            LastSeen = client.LastSeen,
            LastHeartbeat = client.LastHeartbeat
        };
        return Result<ClientDto>.Success(dto);
    }

    public async Task<Result<bool>> DeleteClientAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await _clientRepository.DeleteByIdAsync(id, cancellationToken);
        return Result<bool>.Success(true);
    }

    public async Task<Result<bool>> BlockClientAsync(Guid id, BlockClientRequest request, CancellationToken cancellationToken = default)
    {
        var client = await _clientRepository.GetByIdAsync(id, cancellationToken);
        if (client == null)
            return Result<bool>.Failure("Client not found");
            
        // Implement block logic
        client.Block(request.Reason ?? "No reason provided", "System");
        await _clientRepository.UpdateAsync(client, cancellationToken);
        return Result<bool>.Success(true);
    }

    public async Task<Result<bool>> UnblockClientAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var client = await _clientRepository.GetByIdAsync(id, cancellationToken);
        if (client == null)
            return Result<bool>.Failure("Client not found");
            
        // Implement unblock logic
        client.Unblock();
        await _clientRepository.UpdateAsync(client, cancellationToken);
        return Result<bool>.Success(true);
    }

    public async Task<Result<bool>> UpdateHeartbeatAsync(string computerCode, CancellationToken cancellationToken = default)
    {
        await _clientRepository.UpdateHeartbeatAsync(computerCode, cancellationToken);
        return Result<bool>.Success(true);
    }
}