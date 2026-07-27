

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
        private readonly ILogger logger;

        public CancelAppointmentCommandHandler(IApplicationDbContext context , ILogger logger)
        {
            this.context = context;
            this.logger = logger;
        }

        public async Task<Unit> Handle(CancelAppointmentCommand request , CancellationToken cancellationToken)
        {
            var appointment = await context.Appointments
                .Include(x => x.AvailabilitySlot)
                .FirstOrDefaultAsync(x => x.Id == request.AppointmentId);

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
                throw new BadRequestException("Appointment is already completed");
            }

            appointment.Status = AppointmentStatus.Cancelled;

            appointment.AvailabilitySlot.IsBooked = false;

            await context.SaveChangesAsync(cancellationToken);

            logger.LogWarning($"Attempt to cancel completed appointment {appointment.Id}.");

            return Unit.Value;

        }
    }
}
