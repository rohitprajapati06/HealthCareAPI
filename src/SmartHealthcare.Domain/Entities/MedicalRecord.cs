using SmartHealthcare.Domain.Common;


namespace SmartHealthcare.Domain.Entities
{
    public class MedicalRecord:BaseEntity
    {
        public Guid PatientId { get; set; }

        public PatientProfile Patient{ get; set; }  

        public Guid HospitalId { get; set; }

        public Hospital Hospital { get; set; }  

        public string FileName { get; set; }

        public string FileUrl { get; set; }

        public string RecordType { get; set; }


    }
}
