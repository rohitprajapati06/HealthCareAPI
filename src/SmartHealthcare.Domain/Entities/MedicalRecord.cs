using SmartHealthcare.Domain.Common;


namespace SmartHealthcare.Domain.Entities
{
    public class MedicalRecord:BaseEntity
    {
        public Guid PatientId { get; set; }

        public PatientProfile Patient { get; set; } = default!;

        public Guid HospitalId { get; set; }

        public Hospital Hospital { get; set; } = default!;

        public string FileName { get; set; }

        public string FileUrl { get; set; }

        public string RecordType { get; set; }


    }
}
