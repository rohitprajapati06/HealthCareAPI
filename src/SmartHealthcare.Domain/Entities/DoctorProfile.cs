using SmartHealthcare.Domain.Common;
using SmartHealthcare.Domain.Enums;


namespace SmartHealthcare.Domain.Entities
{
    public class DoctorProfile:BaseEntity
    {
        public Guid UserId { get; set; }    

        public ApplicationUser User { get; set; }

        public Guid HospitalId { get; set; }

        public Hospital Hospital { get; set; }

        public string Specialization { get; set; }

        public int ExperienceYears { get; set; }    

        public decimal ConsultationFee { get; set; }

        public string Qualification {  get; set; }

        public DoctorApprovalStatus ApprovalStatus { get; set; } = DoctorApprovalStatus.Pending;

        public ICollection<Appointment> Appointments { get; set; }

        public ICollection<Prescription> Prescriptions { get; set; }



    }

}
