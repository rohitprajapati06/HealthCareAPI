using MediatR;


namespace SmartHealthcare.Application.Features.Appointments.Commands.BookAppointment
{
    public class BookAppointmentCommand:IRequest<Guid>
    {
        public Guid DoctorId { get; set; }

        public Guid PatientId { get; set; }

        public Guid HospitalId { get; set; }

        public Guid AvailabilitySlotId { get; set; }

        public string? Notes { get; set; } = string.Empty;
    }
}
