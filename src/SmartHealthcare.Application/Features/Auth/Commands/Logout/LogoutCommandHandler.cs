using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using SmartHealthcare.Application.Contracts.Identity;
using SmartHealthcare.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartHealthcare.Application.Features.Auth.Commands.Logout
{
    public class LogoutCommandHandler:IRequestHandler<LogoutCommand,bool>
    {
        private readonly IJwtTokenService jwtTokenService;
        private readonly ILogger<LogoutCommandHandler> logger;

        public LogoutCommandHandler(IJwtTokenService jwtTokenService , ILogger<LogoutCommandHandler> logger)
        {
            this.jwtTokenService = jwtTokenService;
            this.logger = logger;
        }

        public async Task<bool> Handle(LogoutCommand request , CancellationToken cancellationToken) 
        {
            await jwtTokenService.LogoutAsync(request.RefreshToken);
            logger.LogInformation($"User has been logged out");
            return true;
        }

    }
}
