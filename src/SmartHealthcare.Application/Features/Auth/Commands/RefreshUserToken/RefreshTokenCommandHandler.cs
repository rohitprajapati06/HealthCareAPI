

using MediatR;
using Microsoft.Extensions.Logging;
using SmartHealthcare.Application.Contracts.Identity;
using SmartHealthcare.Application.Features.Auth.Responses;


namespace SmartHealthcare.Application.Features.Auth.Commands.RefreshUserToken
{
    public class RefreshTokenCommandHandler: IRequestHandler<RefreshTokenCommand,AuthResponse>
    {
        private readonly IJwtTokenService jwtTokenService;
        private readonly ILogger<RefreshTokenCommandHandler> logger;

        public RefreshTokenCommandHandler(IJwtTokenService jwtTokenService,ILogger<RefreshTokenCommandHandler> logger)
        {
            this.jwtTokenService = jwtTokenService;
            this.logger = logger;
        }

        public async Task<AuthResponse> Handle (RefreshTokenCommand request , CancellationToken cancellationToken)
        {
            logger.LogInformation("Refresh token handler has been invoked");
            return await jwtTokenService.RefreshTokenAsync(request.RefreshToken);
        }
    }
}
