

using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartHealthcare.Application.Common.Exceptions;
using SmartHealthcare.Application.Contracts.Persistence;
using SmartHealthcare.Domain.Entities;


namespace SmartHealthcare.Application.Features.Appointments.Commands.BookAppointment
{
    public class BookAppointmentCommandHandler : IRequestHandler<BookAppointmentCommand, Guid>
    {
        private readonly IApplicationDbContext context;
        private readonly ILogger logger;

        public BookAppointmentCommandHandler(IApplicationDbContext context , ILogger logger)
        {
            this.context = context;
            this.logger = logger;
        }

        public async Task<Guid> Handle(BookAppointmentCommand request , CancellationToken cancellationToken)
        {
            var doctor = await context.DoctorProfiles.FirstOrDefaultAsync(x => x.Id == request.DoctorId , cancellationToken);
            
            if(doctor == null)
            {
                throw new NotFoundException("Doctor not found");
            }

            var patient = await context.PatientProfiles.FirstOrDefaultAsync(x => x.Id == request.PatientId, cancellationToken);

            if(patient == null)
            {
                throw new NotFoundException("Patient not found");
            }

            var hospital = await context.Hospitals.FirstOrDefaultAsync(x => x.Id == request.HospitalId, cancellationToken);

            if(hospital == null)
            {
                throw new NotFoundException("Hospital not found");
            }

            var slot = await context.AvailabilitySlots.FirstOrDefaultAsync(x => x.Id == request.AvailabilitySlotId, cancellationToken);
            
            if(slot == null)
            {
                throw new NotFoundException("Slot not found");
            }

            if(slot.DoctorId != request.DoctorId)
            {
                throw new ForbiddenException("Slot does not belong to the doctor");
            }

            if (slot.IsBooked)
            {
                throw new ConflictException("Slot is already booked ");
            }

            var appointment = new Appointment
            {
                DoctorId = request.DoctorId,
                PatientId = request.PatientId,
                HospitalId = request.HospitalId,
                AvailabilitySlotId = request.AvailabilitySlotId,
                AppointmentDate = slot.Date.ToDateTime(slot.StartTime),
                Notes = request.Notes

            };

            await context.Appointments.AddAsync(appointment);

            slot.IsBooked = true;

            await context.SaveChangesAsync();

            logger.LogInformation($"Appointment {appointment.Id} booked.");

            return appointment.Id;
        }
    }
}
