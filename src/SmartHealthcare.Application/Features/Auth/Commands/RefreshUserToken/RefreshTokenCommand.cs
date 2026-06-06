

using MediatR;
using SmartHealthcare.Application.Features.Auth.Responses;

namespace SmartHealthcare.Application.Features.Auth.Commands.RefreshUserToken
{
    public class RefreshTokenCommand:IRequest<AuthResponse>
    {
        public string RefreshToken { get; set; } = string.Empty;
    }
}
