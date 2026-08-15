using SmartHealthcare.Application.Features.Auth.Responses;
using SmartHealthcare.Domain.Entities;


namespace SmartHealthcare.Application.Contracts.Identity
{
    public interface IJwtTokenService
    {
        Task<AuthResponse> GenerateTokenAsync(ApplicationUser user ,CancellationToken cancellationToken = default);

        Task<AuthResponse> RefreshTokenAsync(string refreshtoken, CancellationToken cancellationToken = default);

        Task LogoutAsync(string refreshtoken, CancellationToken cancellationToken = default);
    }
}
