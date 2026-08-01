

using FluentValidation;

namespace SmartHealthcare.Application.Features.MedicalRecords.Commands.UpdateMedicalRecord
{
    public class UpdateMedicalRecordCommandvalidator : AbstractValidator<UpdateMedicalRecordCommand>
    {
        public UpdateMedicalRecordCommandvalidator()
        {
            RuleFor(x => x.File).Must(file => file is null || file.Length > 0).WithMessage("The uploaded file must not be empty");
            RuleFor(x => x.RecordType).NotEmpty();


        }
    }
}
