using FluentValidation;

namespace SmartHealthcare.Application.Features.MedicalRecords.Commands.CreateMedicalRecord
{
    public class CreateMedicalRecordCommandValidator : AbstractValidator<CreateMedicalRecordCommand>
    {
        public CreateMedicalRecordCommandValidator()
        {
            RuleFor(x => x.HospitalId).NotEmpty();
            RuleFor(x => x.File.Length).GreaterThan(0);
            RuleFor(x => x.File).NotNull();
            RuleFor(x => x.RecordType).NotEmpty().MaximumLength(100);

        }
    }
}
