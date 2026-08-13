

using SmartHealthcare.Domain.Entities;

namespace SmartHealthcare.Application.Features.MedicalRecords.Responses
{
    public sealed class MedicalRecordResponse 
    {
        public Guid Id { get; init; }
        public Guid PatientId { get; init; }
        public Guid HospitalId { get; init; }
        public string PatientName { get; init; } = string.Empty;
        public string HospitalName { get; init; } = string.Empty;
        public string FileName { get; init; } = string.Empty;
        public string FileUrl { get; init; } = string.Empty;
        public string RecordType { get; init; } = string.Empty;
        public DateTime CreatedAt { get; init; }
    }
}
