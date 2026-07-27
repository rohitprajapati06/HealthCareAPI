

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
        private readonly ILogger logger;

        public CompleteAppointmentCommandHandler(IApplicationDbContext context , ILogger logger)
        {
            this.context = context;
            this.logger = logger;
        }

        public async Task<Unit> Handle (CompleteAppointmentCommand request , CancellationToken cancellationToken)
        {
            var appointments = await context.Appointments.FirstOrDefaultAsync(x => x.Id == request.AppointmentId);

            if(appointments == null)
            {
                throw new NotFoundException("Appointment not found");
            }

            if(appointments.Status == AppointmentStatus.Cancelled)
            {
                throw new BadRequestException("Appointment is already cancelled");
            }

            if(appointments.Status == AppointmentStatus.Completed)
            {
                throw new BadRequestException("Appointment is already completed");
            }

            appointments.Status = AppointmentStatus.Completed;

            await context.SaveChangesAsync(cancellationToken);

            logger.LogInformation($"Appointment {appointments.Id} completed.");

            return Unit.Value;
        }
    }
}
