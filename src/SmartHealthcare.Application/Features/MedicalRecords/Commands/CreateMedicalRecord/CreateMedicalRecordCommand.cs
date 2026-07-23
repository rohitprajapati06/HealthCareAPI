using MediatR;

namespace SmartHealthcare.Application.Features.MedicalRecords.Commands.CreateMedicalRecord
{
    public class CreateMedicalRecordCommand : IRequest<Guid>
    {
        public Guid Id { get; set; }

        public Guid HospitalId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FileUrl { get; set; } = string.Empty;
        public string RecordType { get; set; } = string.Empty;
    }
}
