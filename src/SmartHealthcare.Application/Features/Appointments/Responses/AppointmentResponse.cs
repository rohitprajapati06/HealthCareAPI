
using SmartHealthcare.Domain.Entities;

namespace SmartHealthcare.Application.Features.Appointments.Responses
{
    public sealed class AppointmentResponse
    {
        public Guid Id { get; init ; }


        public Guid DoctorId { get; init; }
        public string DoctorName { get; init; }


        public Guid HospitalId { get; init; }
        public string HospitalName { get; init; }



        public Guid PatientId { get; init; }
        public string PatientName { get; init; }


        public Guid AvailabilitySlotId { get; init; }

        public DateTime AppointmentDate { get; init; }

        public string Status { get; init; } = string.Empty;

        public string? Notes { get; init; }

    }
}
