

using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartHealthcare.Application.Contracts.Persistence;
using SmartHealthcare.Domain.Entities;


namespace SmartHealthcare.Application.Features.Appointments.Commands.BookAppointment
{
    public class BookAppointmentCommandHandler : IRequestHandler<BookAppointmentCommand, Guid>
    {
        private readonly IApplicationDbContext context;

        public BookAppointmentCommandHandler(IApplicationDbContext context)
        {
            this.context = context;
        }

        public async Task<Guid> Handle(BookAppointmentCommand request , CancellationToken cancellationToken)
        {
            var doctor = await context.DoctorProfiles.FirstOrDefaultAsync(x => x.Id == request.DoctorId , cancellationToken);
            
            if(doctor == null)
            {
                throw new Exception("Doctor not found");
            }

            var patient = await context.PatientProfiles.FirstOrDefaultAsync(x => x.Id == request.PatientId, cancellationToken);

            if(patient == null)
            {
                throw new Exception("Patient not found");
            }

            var hospital = await context.Hospitals.FirstOrDefaultAsync(x => x.Id == request.HospitalId, cancellationToken);

            if(hospital == null)
            {
                throw new Exception("Hospital not found");
            }

            var slot = await context.AvailabilitySlots.FirstOrDefaultAsync(x => x.Id == request.AvailabilitySlotId, cancellationToken);
            
            if(slot == null)
            {
                throw new Exception("Slot not found");
            }

            if(slot.DoctorId != request.DoctorId)
            {
                throw new Exception("Slot does not belong to th doctor");
            }

            if (slot.IsBooked)
            {
                throw new Exception("Slot is already booked ");
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

            return appointment.Id;
        }
    }
}
