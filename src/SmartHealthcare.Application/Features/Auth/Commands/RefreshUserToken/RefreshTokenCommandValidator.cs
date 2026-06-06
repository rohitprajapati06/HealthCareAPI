

using FluentValidation;

namespace SmartHealthcare.Application.Features.Auth.Commands.RefreshUserToken
{
    public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
    {
        public RefreshTokenCommandValidator()
        {
            RuleFor(x => x.RefreshToken).NotEmpty();
        }
    }
}
