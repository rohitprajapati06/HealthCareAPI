using MediatR;
using Microsoft.AspNetCore.Identity;
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

        public LogoutCommandHandler(IJwtTokenService jwtTokenService)
        {
            this.jwtTokenService = jwtTokenService;
        }

        public async Task<bool> Handle(LogoutCommand request , CancellationToken cancellationToken) 
        {
            await jwtTokenService.LogoutAsync(request.RefreshToken);
            return true;
        }

    }
}
