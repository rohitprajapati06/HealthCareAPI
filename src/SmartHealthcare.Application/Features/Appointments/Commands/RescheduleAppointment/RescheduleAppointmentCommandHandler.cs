using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartHealthcare.Application.Common.Exceptions;
using SmartHealthcare.Application.Contracts.Identity;
using SmartHealthcare.Application.Contracts.Persistence;
using SmartHealthcare.Domain.Enums;

namespace SmartHealthcare.Application.Features.Appointments.Commands.RescheduleAppointment
{
    public class RescheduleAppointmentCommandHandler : IRequestHandler<RescheduleAppointmentCommand, Unit>
    {
        private readonly IApplicationDbContext context;
        private readonly ILogger<RescheduleAppointmentCommandHandler> logger;
        private readonly ICurrentUserService currentUserService;

        public RescheduleAppointmentCommandHandler(IApplicationDbContext context, ILogger<RescheduleAppointmentCommandHandler> logger, ICurrentUserService currentUserService)
        {
            this.context = context;
            this.logger = logger;
            this.currentUserService = currentUserService;
        }

        public async Task<Unit> Handle(RescheduleAppointmentCommand request, CancellationToken cancellationToken)
        {
            var appointment = await context.Appointments.FirstOrDefaultAsync(x => x.Id == request.AppointmentId, cancellationToken);

            if (appointment == null)
            {
                throw new NotFoundException("Appointment not found");
            }

            if (currentUserService.IsInRole(UserRoles.Patient))
            {
                var ownPatientProfileId = await context.PatientProfiles
                    .AsNoTracking()
                    .Where(p => p.UserId == currentUserService.UserId)
                    .Select(p => (Guid?)p.Id)
                    .FirstOrDefaultAsync(cancellationToken);

                if (ownPatientProfileId == null || ownPatientProfileId != appointment.PatientId)
                {
                    throw new ForbiddenException("You are not allowed to reschedule this appointment.");
                }
            }
            else if (currentUserService.IsInRole(UserRoles.Doctor))
            {
                var ownDoctorProfileId = await context.DoctorProfiles
                    .AsNoTracking()
                    .Where(d => d.UserId == currentUserService.UserId)
                    .Select(d => (Guid?)d.Id)
                    .FirstOrDefaultAsync(cancellationToken);

                if (ownDoctorProfileId == null || ownDoctorProfileId != appointment.DoctorId)
                {
                    throw new ForbiddenException("You are not allowed to reschedule this appointment.");
                }
            }

            if (appointment.Status != AppointmentStatus.Pending)
            {
                throw new BadRequestException("Only pending appointments can be rescheduled.");
            }

            if (appointment.Status == AppointmentStatus.Completed)
            {
                throw new BadRequestException("Appointment has been compeleted");
            }

            if (appointment.Status == AppointmentStatus.Cancelled)
            {
                throw new BadRequestException("Appointment has been cancelled");
            }


            var newSlot = await context.AvailabilitySlots.FirstOrDefaultAsync(x => x.Id == request.AvailabilitySlotId, cancellationToken);

            if (newSlot == null)
            {
                throw new NotFoundException("Slot not found");
            }

            if (newSlot.IsBooked)
            {
                throw new ConflictException("Slot is already book");
            }

            if (newSlot.DoctorId != appointment.DoctorId)
            {
                throw new ForbiddenException("The slot does not belong to the doctor");

            }

            var oldSlot = await context.AvailabilitySlots.FirstOrDefaultAsync(x => x.Id == appointment.AvailabilitySlotId, cancellationToken);

            if (oldSlot == null)
            {
                throw new NotFoundException("Current availability slot not found.");
            }


            var newAppointmentDate = newSlot.Date.ToDateTime(newSlot.StartTime);

            if (newAppointmentDate <= DateTime.Now)
            {
                throw new BadRequestException("Cannot reschedule to a past slot.");
            }

            bool patientBusy = await context.Appointments.AnyAsync(x => x.PatientId == appointment.PatientId
                   && x.Id != appointment.Id && x.AppointmentDate == newAppointmentDate, cancellationToken);

            if (patientBusy)
            {
                throw new ConflictException("Patient already has another appointment at this time.");
            }

            bool doctorBusy = await context.Appointments.AnyAsync(x => x.DoctorId == appointment.DoctorId
                    && x.Id != appointment.Id && x.AppointmentDate == newAppointmentDate, cancellationToken);

            if (doctorBusy)
            {
                throw new ConflictException("Doctor already has another appointment at this time.");
            }

            oldSlot.IsBooked = false;
            newSlot.IsBooked = true;

            appointment.AvailabilitySlotId = newSlot.Id;
            appointment.AppointmentDate = newAppointmentDate;

            await context.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Appointment {AppointmentId} rescheduled from Slot {OldSlotId} to Slot {NewSlotId}.",
                appointment.Id, oldSlot.Id, newSlot.Id);

            return Unit.Value;


        }
    }
}