

using FluentValidation;
using SmartHealthcare.Application.Features.Auth.Commands.RegisterPatient;

namespace SmartHealthcare.Application.Features.Auth.Commands.RegisterDoctor
{
    public class RegisterDoctorCommandValidator:AbstractValidator<RegisterDoctorCommand>
    {
        public RegisterDoctorCommandValidator()
        {
            RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
            RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
            RuleFor(x => x.PhoneNumber).NotEmpty().MaximumLength(10);
            RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
            RuleFor(x => x.Password).NotEmpty().MinimumLength(8).MaximumLength(128);
            RuleFor(x => x.Specialization).NotEmpty().MaximumLength(100);
            RuleFor(x => x.HospitalId).NotEmpty();

        }
    }
}
