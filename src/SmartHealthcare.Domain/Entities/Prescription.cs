using SmartHealthcare.Domain.Common;


namespace SmartHealthcare.Domain.Entities
{
    public class Prescription: BaseEntity
    {
        public Guid AppointmentId { get; set; }

        public Appointment Appointment { get; set; } = default!;

        public Guid DoctorId { get; set; }

        public DoctorProfile DoctorProfile { get; set; } = default!;

        public string Medication {  get; set; } = string.Empty;

        public string Instructions { get; set; } = string.Empty;

    }
}
