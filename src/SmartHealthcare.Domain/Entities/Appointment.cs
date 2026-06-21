using SmartHealthcare.Domain.Common;
using SmartHealthcare.Domain.Enums;


namespace SmartHealthcare.Domain.Entities
{
    public class Appointment:BaseEntity
    {
        public Guid DoctorId { get; set; }
        public DoctorProfile Doctor { get; set; }    


        public Guid PatientId { get; set; }
        public PatientProfile Patient { get; set; }

 
        public Guid HospitalId { get; set; }
        public Hospital Hospital {  get; set; }


        public DateTime AppointmentDate { get; set; }

        public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;

        public string? Notes { get; set; }  = string.Empty;

        public Prescription? Prescription { get; set; }  
    }
}
