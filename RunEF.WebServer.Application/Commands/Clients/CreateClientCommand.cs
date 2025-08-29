using MediatR;
using RunEF.WebServer.Application.Common;
using RunEF.WebServer.Application.DTOs;
using RunEF.WebServer.Application.Interfaces;

namespace RunEF.WebServer.Application.Commands.Clients;

public record CreateClientCommand(CreateClientRequest Request) : IRequest<Result<ClientDto>>;

public class CreateClientCommandHandler(IClientService clientService) : IRequestHandler<CreateClientCommand, Result<ClientDto>>
{
    public async Task<Result<ClientDto>> Handle(CreateClientCommand request, CancellationToken cancellationToken)
    {
        return await clientService.CreateClientAsync(request.Request, cancellationToken);
    }
}