using MediatR;
using RunEF.WebServer.Application.Common;
using RunEF.WebServer.Application.DTOs;
using RunEF.WebServer.Application.Interfaces;

namespace RunEF.WebServer.Application.Commands.Clients;

public record BlockClientCommand(Guid Id, BlockClientRequest Request) : IRequest<Result<bool>>;

public class BlockClientCommandHandler(IClientService clientService) : IRequestHandler<BlockClientCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(BlockClientCommand request, CancellationToken cancellationToken)
    {
        return await clientService.BlockClientAsync(request.Id, request.Request, cancellationToken);
    }
}