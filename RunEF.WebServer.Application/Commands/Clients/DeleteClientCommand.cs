using MediatR;
using RunEF.WebServer.Application.Common;
using RunEF.WebServer.Application.Interfaces;

namespace RunEF.WebServer.Application.Commands.Clients;

public record DeleteClientCommand(Guid Id) : IRequest<Result<bool>>;

public class DeleteClientCommandHandler(IClientService clientService) : IRequestHandler<DeleteClientCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(DeleteClientCommand request, CancellationToken cancellationToken)
    {
        return await clientService.DeleteClientAsync(request.Id, cancellationToken);
    }
}