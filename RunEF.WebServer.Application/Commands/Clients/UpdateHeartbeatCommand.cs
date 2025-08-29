using MediatR;
using RunEF.WebServer.Application.Common;
using RunEF.WebServer.Application.Interfaces;

namespace RunEF.WebServer.Application.Commands.Clients;

public record UpdateHeartbeatCommand(string ComputerCode) : IRequest<Result<bool>>;

public class UpdateHeartbeatCommandHandler(IClientService clientService) : IRequestHandler<UpdateHeartbeatCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(UpdateHeartbeatCommand request, CancellationToken cancellationToken)
    {
        return await clientService.UpdateHeartbeatAsync(request.ComputerCode, cancellationToken);
    }
}