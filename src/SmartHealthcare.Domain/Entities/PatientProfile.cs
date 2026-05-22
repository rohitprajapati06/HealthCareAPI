using SmartHealthcare.Domain.Common;


namespace SmartHealthcare.Domain.Entities
{
    public class PatientProfile: BaseEntity
    {
        public Guid UserId { get; set; }

        public ApplicationUser User { get; set; }

        public DateTime DateOfBirth { get; set; }

        public string Gender { get; set; }

        public string BloodGroup { get; set; }

        public string Allergies { get; set; }

        public string ExistingConditions { get; set; }

        public ICollection<Appointment> Appointments { get; set; }

        public ICollection<MedicalRecord> MedicalRecords { get; set; }





    }
}
