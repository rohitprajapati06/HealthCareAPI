using FluentValidation;

namespace SmartHealthcare.Application.Features.MedicalRecords.Commands.CreateMedicalRecord
{
    public class CreateMedicalRecordCommandValidator : AbstractValidator<CreateMedicalRecordCommand>
    {
        public CreateMedicalRecordCommandValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.HospitalId).NotEmpty();
            RuleFor(x => x.FileName).NotEmpty().MaximumLength(255);
            RuleFor(x => x.FileUrl).NotEmpty();
            RuleFor(x => x.RecordType).NotEmpty().MaximumLength(100);

        }
    }
}
