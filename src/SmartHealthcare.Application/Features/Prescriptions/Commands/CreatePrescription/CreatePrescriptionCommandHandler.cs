using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartHealthcare.Application.Common.Exceptions;
using SmartHealthcare.Application.Contracts.Identity;
using SmartHealthcare.Application.Contracts.Persistence;
using SmartHealthcare.Domain.Entities;
using SmartHealthcare.Domain.Enums;

namespace SmartHealthcare.Application.Features.Prescriptions.Commands.CreatePrescription
{
    public class CreatePrescriptionCommandHandler : IRequestHandler<CreatePrescriptionCommand, Guid>
    {
        private readonly IApplicationDbContext context;
        private readonly ILogger<CreatePrescriptionCommandHandler> logger;
        private readonly ICurrentUserService currentUserService;

        public CreatePrescriptionCommandHandler(IApplicationDbContext context, ILogger<CreatePrescriptionCommandHandler> logger, ICurrentUserService currentUserService)
        {
            this.context = context;
            this.logger = logger;
            this.currentUserService = currentUserService;
        }

        public async Task<Guid> Handle(CreatePrescriptionCommand request, CancellationToken cancellationToken)
        {
            var appointments = await context.Appointments
                .Include(x => x.Prescription)
                .FirstOrDefaultAsync(x => x.Id == request.AppointmentId, cancellationToken);

            if (appointments == null)
            {
                throw new NotFoundException("Appointment not found");
            }

            if (appointments.Status != AppointmentStatus.Completed)
            {
                throw new ConflictException("Appointment is not completed yet");
            }

            if (appointments.Prescription != null)
            {
                throw new BadRequestException("Prescription has been already provided");
            }

            if (appointments.DoctorId != request.DoctorId)
            {
                throw new ForbiddenException("Only the assigned doctor can create a prescription.");
            }

            // The check above only confirms request.DoctorId matches the
            // appointment. Confirm the caller actually IS that doctor.
            if (currentUserService.IsInRole(UserRoles.Doctor))
            {
                var callingDoctor = await context.DoctorProfiles
                    .AsNoTracking()
                    .FirstOrDefaultAsync(d => d.Id == request.DoctorId, cancellationToken);

                if (callingDoctor == null || callingDoctor.UserId != currentUserService.UserId)
                {
                    throw new ForbiddenException("Only the assigned doctor can create a prescription.");
                }
            }

            var prescription = new Prescription
            {
                AppointmentId = request.AppointmentId,
                DoctorId = request.DoctorId,
                Medication = request.Medication,
                Instructions = request.Instructions,
                CreatedAt = DateTime.UtcNow
            };

            await context.Prescriptions.AddAsync(prescription, cancellationToken);

            await context.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Prescription {PrescriptionId} created successfully for Appointment {AppointmentId} by Doctor {DoctorId}.",
                prescription.Id, prescription.AppointmentId, prescription.DoctorId);

            return prescription.Id;
        }
    }
}