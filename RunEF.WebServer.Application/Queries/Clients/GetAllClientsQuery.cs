using MediatR;
using RunEF.WebServer.Application.Common;
using RunEF.WebServer.Application.DTOs;
using RunEF.WebServer.Application.Interfaces;

namespace RunEF.WebServer.Application.Queries.Clients;

public record GetAllClientsQuery : IRequest<Result<IEnumerable<ClientDto>>>;

public class GetAllClientsQueryHandler(IClientService clientService) : IRequestHandler<GetAllClientsQuery, Result<IEnumerable<ClientDto>>>
{
    public async Task<Result<IEnumerable<ClientDto>>> Handle(GetAllClientsQuery request, CancellationToken cancellationToken)
    {
        return await clientService.GetAllClientsAsync(cancellationToken);
    }
}