
using MediatR;

namespace SmartHealthcare.Application.Features.Appointments.Commands.RescheduleAppointment
{
    public class RescheduleAppointmentCommand : IRequest<Unit>
    {
        public Guid AppointmentId { get; set; }

        public Guid AvailabilitySlotId { get; set; }
    }
}
