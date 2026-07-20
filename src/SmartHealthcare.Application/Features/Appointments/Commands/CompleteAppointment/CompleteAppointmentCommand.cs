using MediatR;

namespace SmartHealthcare.Application.Features.Appointments.Commands.CompleteAppointment
{
    public class CompleteAppointmentCommand : IRequest
    {
        public Guid AppointmentId { get; set; }

    }
}
