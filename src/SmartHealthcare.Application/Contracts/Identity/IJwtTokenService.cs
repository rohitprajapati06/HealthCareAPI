using SmartHealthcare.Application.Features.Auth.Responses;
using SmartHealthcare.Domain.Entities;


namespace SmartHealthcare.Application.Contracts.Identity
{
    public interface IJwtTokenService
    {
        Task<AuthResponse> GenerateTokenAsync(ApplicationUser user);

        Task<AuthResponse> RefreshTokenAsync(string refreshtoken);

        Task LogoutAsync(string refreshtoken);
    }
}
