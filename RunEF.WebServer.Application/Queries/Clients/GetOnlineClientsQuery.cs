using MediatR;
using RunEF.WebServer.Application.Common;
using RunEF.WebServer.Application.DTOs;
using RunEF.WebServer.Application.Interfaces;

namespace RunEF.WebServer.Application.Queries.Clients;

public record GetOnlineClientsQuery : IRequest<Result<IEnumerable<ClientDto>>>;

public class GetOnlineClientsQueryHandler(IClientService clientService) : IRequestHandler<GetOnlineClientsQuery, Result<IEnumerable<ClientDto>>>
{
    public async Task<Result<IEnumerable<ClientDto>>> Handle(GetOnlineClientsQuery request, CancellationToken cancellationToken)
    {
        return await clientService.GetOnlineClientsAsync(cancellationToken);
    }
}