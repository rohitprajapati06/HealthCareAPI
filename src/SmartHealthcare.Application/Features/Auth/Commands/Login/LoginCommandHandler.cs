using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using SmartHealthcare.Application.Contracts.Identity;
using SmartHealthcare.Application.Contracts.Persistence;
using SmartHealthcare.Application.Features.Auth.Responses;
using SmartHealthcare.Domain.Entities;


namespace SmartHealthcare.Application.Features.Auth.Commands.Login
{
    public class LoginCommandHandler:IRequestHandler<LoginCommand,AuthResponse>
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IJwtTokenService jwtTokenService;
        private readonly IApplicationDbContext context;
        private readonly ILogger<LoginCommandHandler> logger;

        public LoginCommandHandler(UserManager<ApplicationUser> userManager , IJwtTokenService jwtTokenService ,IApplicationDbContext context , ILogger<LoginCommandHandler> logger)
        {
            this.userManager = userManager;
            this.jwtTokenService = jwtTokenService;
            this.context = context;
            this.logger = logger;
        }
        public async Task<AuthResponse> Handle(LoginCommand request , CancellationToken cancellationToken)
        {
            var user = await userManager.FindByEmailAsync(request.Email);

            if(user == null)
            {
                throw new UnauthorizedAccessException("No User Found");
            }

            var isValidPassword = await userManager.CheckPasswordAsync(user, request.Password);

            if (!isValidPassword)
            {
                throw new UnauthorizedAccessException("Invalid Credentails");
            }

            var authresponse = await jwtTokenService.GenerateTokenAsync(user);

            var refreshtoken = new RefreshToken
            {
                UserId = user.Id,
                Token = authresponse.RefreshToken,
                ExpiryDate = DateTime.UtcNow.AddDays(7)
            };

            await context.RefreshTokens.AddAsync(refreshtoken);

            await context.SaveChangesAsync(cancellationToken);

            logger.LogInformation($"{user.Id} user has loggen in successfully"); 

            return authresponse;
        }
    }
}
