using MediatR;
using RunEF.WebServer.Application.Common;
using RunEF.WebServer.Application.DTOs;
using RunEF.WebServer.Application.Interfaces;

namespace RunEF.WebServer.Application.Commands.Auth;

public record RefreshTokenCommand(string RefreshToken) : IRequest<Result<LoginResponse>>;

public class RefreshTokenCommandHandler(IAuthService authService) : IRequestHandler<RefreshTokenCommand, Result<LoginResponse>>
{
    public async Task<Result<LoginResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        return await authService.RefreshTokenAsync(request.RefreshToken, cancellationToken);
    }
}