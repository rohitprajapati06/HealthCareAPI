using FluentValidation;

namespace SmartHealthcare.Application.Features.MedicalRecords.Commands.CreateMedicalRecord
{
    public class CreateMedicalRecordCommandValidator : AbstractValidator<CreateMedicalRecordCommand>
    {
        public CreateMedicalRecordCommandValidator()
        {
            RuleFor(x => x.HospitalId).NotEmpty();

            RuleFor(x => x.File)
            .NotNull()
            .WithMessage("Medical record file is required.");

            RuleFor(x => x.File)
                .Must(file => file != null && file.Length > 0)
                .WithMessage("Medical record file cannot be empty.");

            RuleFor(x => x.RecordType).NotEmpty().MaximumLength(100);

        }
    }
}
