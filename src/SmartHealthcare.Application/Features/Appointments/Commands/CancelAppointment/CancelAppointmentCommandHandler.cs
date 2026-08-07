

using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartHealthcare.Application.Common.Exceptions;
using SmartHealthcare.Application.Contracts.Persistence;
using SmartHealthcare.Domain.Entities;
using SmartHealthcare.Domain.Enums;

namespace SmartHealthcare.Application.Features.Appointments.Commands.CancelAppointment
{
    public class CancelAppointmentCommandHandler : IRequestHandler<CancelAppointmentCommand,Unit>
    {
        private readonly IApplicationDbContext context;
        private readonly ILogger<CancelAppointmentCommandHandler> logger;

        public CancelAppointmentCommandHandler(IApplicationDbContext context , ILogger<CancelAppointmentCommandHandler> logger)
        {
            this.context = context;
            this.logger = logger;
        }

        public async Task<Unit> Handle(CancelAppointmentCommand request , CancellationToken cancellationToken)
        {
            var appointment = await context.Appointments.FirstOrDefaultAsync(x => x.Id == request.AppointmentId,cancellationToken);

            if(appointment == null)
            {
                throw new NotFoundException("Appointment not found");
            }

            if(appointment.Status == AppointmentStatus.Cancelled)
            {
                throw new BadRequestException("Appointment is already cancelled");
            }

            if(appointment.Status == AppointmentStatus.Completed)
            {
                throw new BadRequestException("Completed appointments cannot be cancelled.");
            }

            var slot = await context.AvailabilitySlots.FirstOrDefaultAsync(x => x.Id == appointment.AvailabilitySlotId,cancellationToken);

            if (slot == null)
            {
                throw new NotFoundException("Availability slot not found.");
            }

            appointment.Status = AppointmentStatus.Cancelled;

            slot.IsBooked = false;

            await context.SaveChangesAsync(cancellationToken);

            logger.LogWarning("Appointment {AppointmentId} cancelled successfully.",appointment.Id);

            return Unit.Value;

        }
    }
}
