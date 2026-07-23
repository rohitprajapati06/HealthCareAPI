

using SmartHealthcare.Domain.Entities;

namespace SmartHealthcare.Application.Features.MedicalRecords.Responses
{
    public class MedicalRecordResponse 
    {
        public Guid Id { get; set; }
        public Guid PatientId { get; set; }
        public Guid HospitalId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string HospitalName { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string FileUrl { get; set; } = string.Empty;
        public string RecordType { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
