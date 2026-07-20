
using SmartHealthcare.Domain.Entities;

namespace SmartHealthcare.Application.Features.Appointments.Responses
{
    public class AppointmentResponse
    {
        public Guid Id { get; set; }


        public Guid DoctorId { get; set; }
        public string DoctorName { get; set; }


        public Guid HospitalId { get; set; }
        public string HospitalName { get; set; }



        public Guid PatientId { get; set; }
        public string PatientName { get; set; }


        public Guid AvailabilitySlotId { get; set; }

        public DateTime AppointmentDate { get; set; }

        public string Status { get; set; }

        public string? Notes { get; set; }

    }
}
