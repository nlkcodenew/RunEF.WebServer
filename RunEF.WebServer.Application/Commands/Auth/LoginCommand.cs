using MediatR;
using RunEF.WebServer.Application.Common;
using RunEF.WebServer.Application.DTOs;
using RunEF.WebServer.Application.Interfaces;

namespace RunEF.WebServer.Application.Commands.Auth;

public record LoginCommand(LoginRequest Request) : IRequest<Result<LoginResponse>>;

public class LoginCommandHandler(IAuthService authService) : IRequestHandler<LoginCommand, Result<LoginResponse>>
{
    public async Task<Result<LoginResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        return await authService.LoginAsync(request.Request, cancellationToken);
    }
}