
using FluentValidation;

namespace SmartHealthcare.Application.Features.Prescriptions.Commands.UpdatePrescription
{
    public class UpdatePrescriptionCommandValidator : AbstractValidator<UpdatePrescriptionCommand>
    {
        public UpdatePrescriptionCommandValidator()
        {
            RuleFor(x => x.PrescriptionId).NotEmpty();
            RuleFor(x => x.Instruction).NotEmpty().MaximumLength(1000);
            RuleFor(x => x.Medication).NotEmpty().MaximumLength(1000);
        }
    }
}
