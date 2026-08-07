

using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartHealthcare.Application.Common.Exceptions;
using SmartHealthcare.Application.Contracts.Persistence;
using SmartHealthcare.Domain.Entities;
using SmartHealthcare.Domain.Enums;

namespace SmartHealthcare.Application.Features.Appointments.Commands.CompleteAppointment
{
    public class CompleteAppointmentCommandHandler : IRequestHandler<CompleteAppointmentCommand,Unit>
    {
        private readonly IApplicationDbContext context;
        private readonly ILogger<CompleteAppointmentCommandHandler> logger;

        public CompleteAppointmentCommandHandler(IApplicationDbContext context , ILogger<CompleteAppointmentCommandHandler> logger)
        {
            this.context = context;
            this.logger = logger;
        }

        public async Task<Unit> Handle (CompleteAppointmentCommand request , CancellationToken cancellationToken)
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
                throw new BadRequestException("Appointment is already completed");
            }

            if (appointment.AppointmentDate > DateTime.Now)
            {
                throw new BadRequestException("Future appointments cannot be completed.");
            }

            appointment.Status = AppointmentStatus.Completed;

            await context.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Appointment {AppointmentId} completed successfully.",appointment.Id);

            return Unit.Value;
        }
    }
}
