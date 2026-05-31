

using FluentValidation;
using SmartHealthcare.Application.Features.Auth.Commands.RegisterPatient;

namespace SmartHealthcare.Application.Features.Auth.Commands.RegisterDoctor
{
    public class RegisterDoctorCommandValidator:AbstractValidator<RegisterDoctorCommand>
    {
        public RegisterDoctorCommandValidator()
        {
            RuleFor(x => x.FirstName).NotEmpty();
            RuleFor(x => x.LastName).NotEmpty();
            RuleFor(x => x.PhoneNumber).NotEmpty();
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
            RuleFor(x => x.Specialization).NotEmpty();
            RuleFor(x => x.HospitalId).NotEmpty();

        }
    }
}
