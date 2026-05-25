using SmartHealthcare.Application.DTOs;
using SmartHealthcare.Application.Features.Auth.Responses;
using SmartHealthcare.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartHealthcare.Application.Contracts.Identity
{
    public interface IJwtTokenService
    {
        Task<AuthResponse> GenerateTokenAsync(ApplicationUser user);
    }
}
