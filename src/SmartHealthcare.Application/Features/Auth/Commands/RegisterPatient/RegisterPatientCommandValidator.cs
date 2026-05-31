

using FluentValidation;

namespace SmartHealthcare.Application.Features.Auth.Commands.RegisterPatient
{
    public class RegisterPatientCommandValidator : AbstractValidator<RegisterPatientCommand>
    {
        public RegisterPatientCommandValidator()
        {
            RuleFor(x => x.FirstName).NotEmpty();

            RuleFor(x => x.LastName).NotEmpty();

            RuleFor(x => x.Email).NotEmpty().EmailAddress();

            RuleFor(x => x.Password).NotEmpty().MinimumLength(8);

            RuleFor(x => x.Gender).NotEmpty();
        }
    }
}
