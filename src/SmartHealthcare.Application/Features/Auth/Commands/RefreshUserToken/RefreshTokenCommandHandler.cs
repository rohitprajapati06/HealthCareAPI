

using MediatR;
using SmartHealthcare.Application.Contracts.Identity;
using SmartHealthcare.Application.Features.Auth.Responses;


namespace SmartHealthcare.Application.Features.Auth.Commands.RefreshUserToken
{
    public class RefreshTokenCommandHandler: IRequestHandler<RefreshTokenCommand,AuthResponse>
    {
        private readonly IJwtTokenService jwtTokenService;

        public RefreshTokenCommandHandler(IJwtTokenService jwtTokenService)
        {
            this.jwtTokenService = jwtTokenService;
        }

        public async Task<AuthResponse> Handle (RefreshTokenCommand request , CancellationToken cancellationToken)
        {
            return await jwtTokenService.RefreshTokenAsync(request.RefreshToken);
        }
    }
}
