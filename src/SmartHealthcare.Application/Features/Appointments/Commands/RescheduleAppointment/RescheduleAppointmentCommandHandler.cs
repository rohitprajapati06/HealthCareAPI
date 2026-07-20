

using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartHealthcare.Application.Contracts.Persistence;
using SmartHealthcare.Domain.Enums;

namespace SmartHealthcare.Application.Features.Appointments.Commands.RescheduleAppointment
{
    public class RescheduleAppointmentCommandHandler : IRequestHandler<RescheduleAppointmentCommand , Unit>
    {
        private readonly IApplicationDbContext context;

        public RescheduleAppointmentCommandHandler(IApplicationDbContext context)
        {
            this.context = context;
        }

        public async Task<Unit> Handle(RescheduleAppointmentCommand request , CancellationToken cancellationToken) 
        {
            var appointments = await context.Appointments
                .Include(x => x.AvailabilitySlot)
                .FirstOrDefaultAsync(x => x.Id == request.AppointmentId);

            if(appointments == null)
            {
                throw new Exception("Appointment not found");
            }

            if(appointments.Status != AppointmentStatus.Pending)
            {
                throw new Exception("Appointment is not Pending");
            }

            if(appointments.Status == AppointmentStatus.Completed)
            {
                throw new Exception("Appointment has been compeleted");
            }

            if(appointments.Status == AppointmentStatus.Cancelled)
            {
                throw new Exception("Appointment has been cancelled");
            }


            var newSlot = await context.AvailabilitySlots.FirstOrDefaultAsync(x => x.Id == request.AvailabilitySlotId);

            if(newSlot == null)
            {
                throw new Exception("Slot not found");
            }

            if (newSlot.IsBooked)
            {
                throw new Exception("Slot is already book");
            }

            if(newSlot.DoctorId != appointments.DoctorId)
            {
                throw new Exception("The slot does not belong to the doctor");

            }

            appointments.AvailabilitySlot.IsBooked = false;

            newSlot.IsBooked = true;

            appointments.AvailabilitySlotId = newSlot.Id;

            appointments.AppointmentDate = newSlot.Date.ToDateTime(newSlot.StartTime);

            await context.SaveChangesAsync(cancellationToken);

            return Unit.Value;


        }
    }
}
