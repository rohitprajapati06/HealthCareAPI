

using FluentValidation;

namespace SmartHealthcare.Application.Features.Auth.Commands.ForgotPassword
{
    public class ForgotPasswordCommandValidator : AbstractValidator<ForgotPasswordCommand>
    {
        public ForgotPasswordCommandValidator()
        {
            RuleFor(x => x.Email).EmailAddress().NotEmpty().MaximumLength(256);
        }
    }
}
