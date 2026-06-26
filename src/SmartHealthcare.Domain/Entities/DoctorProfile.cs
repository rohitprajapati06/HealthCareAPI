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

        public string Specialization { get; set; } = string.Empty;

        public int ExperienceYears { get; set; }    

        public decimal ConsultationFee { get; set; }

        public string Qualification {  get; set; } = string.Empty;

        public DoctorApprovalStatus ApprovalStatus { get; set; } = DoctorApprovalStatus.Pending;

        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

        public ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();

        public ICollection<AvailabilitySlot> AvailabilitySlots { get; set; } = new List<AvailabilitySlot>();


    }

}
