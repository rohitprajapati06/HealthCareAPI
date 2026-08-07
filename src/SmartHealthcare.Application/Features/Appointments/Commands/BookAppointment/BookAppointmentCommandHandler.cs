

using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartHealthcare.Application.Common.Exceptions;
using SmartHealthcare.Application.Contracts.Persistence;
using SmartHealthcare.Domain.Entities;
using SmartHealthcare.Domain.Enums;


namespace SmartHealthcare.Application.Features.Appointments.Commands.BookAppointment
{
    public class BookAppointmentCommandHandler : IRequestHandler<BookAppointmentCommand, Guid>
    {
        private readonly IApplicationDbContext context;
        private readonly ILogger<BookAppointmentCommandHandler> logger;

        public BookAppointmentCommandHandler(IApplicationDbContext context , ILogger<BookAppointmentCommandHandler> logger)
        {
            this.context = context;
            this.logger = logger;
        }

        public async Task<Guid> Handle(BookAppointmentCommand request , CancellationToken cancellationToken)
        {

            // Validate Doctor

            var doctor = await context.DoctorProfiles.FirstOrDefaultAsync(x => x.Id == request.DoctorId , cancellationToken);
            
            if(doctor == null)
            {
                throw new NotFoundException("Doctor not found");
            }

            if (doctor.ApprovalStatus != DoctorApprovalStatus.Approved)
            {
                throw new ForbiddenException("Doctor is not approved.");
            }


            // Validate Patient

            var patient = await context.PatientProfiles.FirstOrDefaultAsync(x => x.Id == request.PatientId, cancellationToken);

            if(patient == null)
            {
                throw new NotFoundException("Patient not found");
            }


            //Validate Hospital

            var hospital = await context.Hospitals.FirstOrDefaultAsync(x => x.Id == request.HospitalId, cancellationToken);

            if(hospital == null)
            {
                throw new NotFoundException("Hospital not found");
            }

            if (doctor.HospitalId != request.HospitalId)
            {
                throw new BadRequestException("Doctor does not belong to the selected hospital.");
            }

            // Validate Slot

            var slot = await context.AvailabilitySlots.FirstOrDefaultAsync(x => x.Id == request.AvailabilitySlotId, cancellationToken);
            
            if(slot == null)
            {
                throw new NotFoundException("Slot not found");
            }

            var slotDateTime = slot.Date.ToDateTime(slot.StartTime);

            if (slotDateTime <= DateTime.Now)
            {
                throw new BadRequestException("Cannot book past appointment slots.");
            }

            if (slot.DoctorId != request.DoctorId)
            {
                throw new ForbiddenException("Slot does not belong to the doctor");
            }

            if (slot.IsBooked)
            {
                throw new ConflictException("Slot is already booked ");
            }


            //Check Patient Double Booking

            bool alreadyBooked =  await context.Appointments
                .AnyAsync(x => x.PatientId == request.PatientId 
                    && x.AppointmentDate == slot.Date.ToDateTime(slot.StartTime),cancellationToken);

            if (alreadyBooked)
            {
                throw new ConflictException("Patient already has an appointment at this time.");
            }
            

            //Check Doctor Double Booking

            bool doctorBusy = await context.Appointments
                .AnyAsync(x => x.DoctorId == request.DoctorId 
                    && x.AppointmentDate == slot.Date.ToDateTime(slot.StartTime),cancellationToken);


            // Create Appointment 

            var appointment = new Appointment
            {
                DoctorId = request.DoctorId,
                PatientId = request.PatientId,
                HospitalId = request.HospitalId,
                AvailabilitySlotId = request.AvailabilitySlotId,
                AppointmentDate = slot.Date.ToDateTime(slot.StartTime),
                Notes = request.Notes

            };

            await context.Appointments.AddAsync(appointment, cancellationToken);

            //Mark slot is Booked

            slot.IsBooked = true;

            await context.SaveChangesAsync();

            logger.LogInformation("Appointment {AppointmentId} booked successfully for Patient {PatientId} with Doctor {DoctorId}."
                ,appointment.Id,appointment.PatientId,appointment.DoctorId);


            return appointment.Id;
        }
    }
}
