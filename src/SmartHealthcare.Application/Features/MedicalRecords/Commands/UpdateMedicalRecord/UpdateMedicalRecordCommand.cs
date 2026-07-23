
using MediatR;

namespace SmartHealthcare.Application.Features.MedicalRecords.Commands.UpdateMedicalRecord
{
    public class UpdateMedicalRecordCommand : IRequest<Unit>
    {
        public Guid Id { get; set; }

        public string FileName { get; set; } = string.Empty;

        public string FileUrl { get; set; } = string.Empty;

        public string RecordType { get; set; } = string.Empty;
    }
}
