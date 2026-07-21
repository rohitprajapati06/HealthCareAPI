using FluentValidation;


namespace SmartHealthcare.Application.Features.Prescriptions.Commands.CreatePrescription
{
    public class CreatePrescriptionCommandValidator : AbstractValidator<CreatePrescriptionCommand>
    {
        public CreatePrescriptionCommandValidator()
        {
            RuleFor(x => x.AppointmentId).NotEmpty();

            RuleFor(x => x.DoctorId).NotEmpty();

            RuleFor(x => x.Instructions).NotEmpty().MaximumLength(1000);

            RuleFor(x => x.Medication).NotEmpty().MaximumLength(1000);
        }
    }
}
