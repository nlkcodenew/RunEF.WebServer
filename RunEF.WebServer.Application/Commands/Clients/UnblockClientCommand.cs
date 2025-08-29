using MediatR;
using RunEF.WebServer.Application.Common;
using RunEF.WebServer.Application.Interfaces;

namespace RunEF.WebServer.Application.Commands.Clients;

public record UnblockClientCommand(Guid Id) : IRequest<Result<bool>>;

public class UnblockClientCommandHandler(IClientService clientService) : IRequestHandler<UnblockClientCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(UnblockClientCommand request, CancellationToken cancellationToken)
    {
        return await clientService.UnblockClientAsync(request.Id, cancellationToken);
    }
}