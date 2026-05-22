using SmartHealthcare.Domain.Common;


namespace SmartHealthcare.Domain.Entities
{
    public class Prescription: BaseEntity
    {
        public Guid AppointmentId { get; set; }

        public Appointment Appointment { get; set; }

        public Guid DoctorId { get; set; }

        public DoctorProfile DoctorProfile { get; set; }

        public string Medication {  get; set; }

        public string Instructions { get; set; }

    }
}
