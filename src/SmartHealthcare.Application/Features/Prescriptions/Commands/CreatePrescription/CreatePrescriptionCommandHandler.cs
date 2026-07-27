
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartHealthcare.Application.Common.Exceptions;
using SmartHealthcare.Application.Contracts.Persistence;
using SmartHealthcare.Application.Features.Prescriptions.Responses;
using SmartHealthcare.Domain.Entities;
using SmartHealthcare.Domain.Enums;

namespace SmartHealthcare.Application.Features.Prescriptions.Commands.CreatePrescription
{
    public class CreatePrescriptionCommandHandler : IRequestHandler<CreatePrescriptionCommand,Guid>
    {
        private readonly IApplicationDbContext context;
        private readonly ILogger logger;

        public CreatePrescriptionCommandHandler(IApplicationDbContext context , ILogger logger)
        {
            this.context = context;
            this.logger = logger;
        }

        public async Task<Guid> Handle(CreatePrescriptionCommand request , CancellationToken cancellationToken)
        {
            var appointments = await context.Appointments
                .Include(x => x.Prescription)
                .FirstOrDefaultAsync(x => x.Id == request.AppointmentId, cancellationToken);

            if(appointments == null)
            {
                throw new NotFoundException("Appointment not found");
            }

            if(appointments.Status != AppointmentStatus.Completed)
            {
                throw new ConflictException("Appointment is not completed yet");
            }

            if(appointments.Prescription != null)
            {
                throw new BadRequestException("Prescription has been already provided");
            }

            if(appointments.DoctorId != request.DoctorId)
            {
                throw new ForbiddenException("Only the assigned doctor can create a prescription.");
            }

            var prescription = new Prescription
            {
                AppointmentId = request.AppointmentId,
                DoctorId = request.DoctorId,
                Medication = request.Medication,
                Instructions = request.Instructions,
            };

           await context.Prescriptions.AddAsync(prescription);
           
           await context.SaveChangesAsync(cancellationToken);

            logger.LogInformation($" Prescriptionn created - {prescription.Id}");
            
            return prescription.Id;
        }
    }
}
