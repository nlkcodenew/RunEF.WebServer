using MediatR;
using RunEF.WebServer.Application.Common;
using RunEF.WebServer.Application.DTOs;
using RunEF.WebServer.Application.Interfaces;

namespace RunEF.WebServer.Application.Queries.Clients;

public record GetClientByIdQuery(Guid Id) : IRequest<Result<ClientDto>>;

public class GetClientByIdQueryHandler(IClientService clientService) : IRequestHandler<GetClientByIdQuery, Result<ClientDto>>
{
    public async Task<Result<ClientDto>> Handle(GetClientByIdQuery request, CancellationToken cancellationToken)
    {
        return await clientService.GetClientByIdAsync(request.Id, cancellationToken);
    }
}