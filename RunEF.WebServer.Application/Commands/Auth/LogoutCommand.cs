using MediatR;
using RunEF.WebServer.Application.Common;
using RunEF.WebServer.Application.Interfaces;

namespace RunEF.WebServer.Application.Commands.Auth;

public record LogoutCommand(int UserId) : IRequest<Result<bool>>;

public class LogoutCommandHandler(IAuthService authService) : IRequestHandler<LogoutCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        return await authService.LogoutAsync(request.UserId, cancellationToken);
    }
}