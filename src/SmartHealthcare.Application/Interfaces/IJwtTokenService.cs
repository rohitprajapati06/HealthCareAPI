using SmartHealthcare.Application.DTOs;
using SmartHealthcare.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartHealthcare.Application.Interfaces
{
    public interface IJwtTokenService
    {
        Task<AuthResponseDto> GenerateTokenAsync(ApplicationUser user);
    }
}
