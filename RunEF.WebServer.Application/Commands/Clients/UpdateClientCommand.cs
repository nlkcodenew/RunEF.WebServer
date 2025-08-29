using MediatR;
using RunEF.WebServer.Application.Common;
using RunEF.WebServer.Application.DTOs;
using RunEF.WebServer.Application.Interfaces;

namespace RunEF.WebServer.Application.Commands.Clients;

public record UpdateClientCommand(Guid Id, UpdateClientRequest Request) : IRequest<Result<ClientDto>>;

public class UpdateClientCommandHandler(IClientService clientService) : IRequestHandler<UpdateClientCommand, Result<ClientDto>>
{
    public async Task<Result<ClientDto>> Handle(UpdateClientCommand request, CancellationToken cancellationToken)
    {
        return await clientService.UpdateClientAsync(request.Id, request.Request, cancellationToken);
    }
}