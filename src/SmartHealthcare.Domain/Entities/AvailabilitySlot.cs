

using SmartHealthcare.Domain.Common;

namespace SmartHealthcare.Domain.Entities
{
    public class AvailabilitySlot:BaseEntity
    {
        public Guid DoctorId { get; set; }

        public DoctorProfile Doctor { get; set; }

        public DateOnly Date { get; set; }

        public TimeOnly StartTime { get; set; }

        public TimeOnly EndTime { get; set; }   

        public bool IsBooked { get; set; } = false;

        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    }
}
