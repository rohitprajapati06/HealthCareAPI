

using FluentValidation;

namespace SmartHealthcare.Application.Features.MedicalRecords.Commands.UpdateMedicalRecord
{
    public class UpdateMedicalRecordCommandvalidator : AbstractValidator<UpdateMedicalRecordCommand>
    {
        public UpdateMedicalRecordCommandvalidator()
        {
            RuleFor(x => x.FileName).NotEmpty();
            RuleFor(x => x.FileUrl).NotEmpty();
            RuleFor(x => x.RecordType).NotEmpty();


        }
    }
}
