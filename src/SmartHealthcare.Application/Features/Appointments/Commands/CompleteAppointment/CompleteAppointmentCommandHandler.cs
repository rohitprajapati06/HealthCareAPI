using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartHealthcare.Application.Common.Exceptions;
using SmartHealthcare.Application.Contracts.Identity;
using SmartHealthcare.Application.Contracts.Persistence;
using SmartHealthcare.Domain.Enums;

namespace SmartHealthcare.Application.Features.Appointments.Commands.CompleteAppointment
{
    public class CompleteAppointmentCommandHandler : IRequestHandler<CompleteAppointmentCommand, Unit>
    {
        private readonly IApplicationDbContext context;
        private readonly ILogger<CompleteAppointmentCommandHandler> logger;
        private readonly ICurrentUserService currentUserService;

        public CompleteAppointmentCommandHandler(IApplicationDbContext context, ILogger<CompleteAppointmentCommandHandler> logger, ICurrentUserService currentUserService)
        {
            this.context = context;
            this.logger = logger;
            this.currentUserService = currentUserService;
        }

        public async Task<Unit> Handle(CompleteAppointmentCommand request, CancellationToken cancellationToken)
        {
            var appointment = await context.Appointments.FirstOrDefaultAsync(x => x.Id == request.AppointmentId, cancellationToken);

            if (appointment == null)
            {
                throw new NotFoundException("Appointment not found");
            }

            // Controller already restricts this to Doctor/HospitalAdmin/SuperAdmin.
            // A Doctor caller may only complete their own appointment.
            if (currentUserService.IsInRole(UserRoles.Doctor))
            {
                var ownDoctorProfileId = await context.DoctorProfiles
                    .AsNoTracking()
                    .Where(d => d.UserId == currentUserService.UserId)
                    .Select(d => (Guid?)d.Id)
                    .FirstOrDefaultAsync(cancellationToken);

                if (ownDoctorProfileId == null || ownDoctorProfileId != appointment.DoctorId)
                {
                    throw new ForbiddenException("You are not allowed to complete this appointment.");
                }
            }

            if (appointment.Status == AppointmentStatus.Cancelled)
            {
                throw new BadRequestException("Appointment is already cancelled");
            }

            if (appointment.Status == AppointmentStatus.Completed)
            {
                throw new BadRequestException("Appointment is already completed");
            }

            if (appointment.AppointmentDate > DateTime.Now)
            {
                throw new BadRequestException("Future appointments cannot be completed.");
            }

            appointment.Status = AppointmentStatus.Completed;

            await context.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Appointment {AppointmentId} completed successfully.", appointment.Id);

            return Unit.Value;
        }
    }
}