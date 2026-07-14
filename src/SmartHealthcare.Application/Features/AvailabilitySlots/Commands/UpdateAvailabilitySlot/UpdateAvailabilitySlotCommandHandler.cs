

using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartHealthcare.Application.Contracts.Persistence;

namespace SmartHealthcare.Application.Features.AvailabilitySlots.Commands.UpdateAvailabilitySlot
{
    public class UpdateAvailabilitySlotCommandHandler : IRequestHandler<UpdateAvailabilitySlotCommand>
    {
        private readonly IApplicationDbContext context;

        public UpdateAvailabilitySlotCommandHandler(IApplicationDbContext context)
        {
            this.context = context;
        }

        public async Task<Unit> Handle(UpdateAvailabilitySlotCommand request, CancellationToken cancellationToken)
        {
            var slot = await context.AvailabilitySlots
               .FirstOrDefaultAsync(x => x.Id == request.SlotId, cancellationToken);

            if(slot == null)
            {
                throw new Exception("There is no slots booked");
            }

            if(slot.DoctorId != request.DoctorId)
            {
                throw new Exception("You cannot update the another doctors slots ");
            }

            if (slot.IsBooked)
            {
                throw new Exception("There is no slot available");
            }

            if(slot.Date < DateOnly.FromDateTime(DateTime.Today))
            {
                throw new Exception("You cannot update the past date slots");
            }

            if(request.EndTime <= request.StartTime)
            {
                throw new Exception("End time must be greater than the start time");
            }

            var overlap = await context.AvailabilitySlots
                .AnyAsync(x => x.Id != slot.Id && x.DoctorId == request.DoctorId && x.Date == request.Date
                     && request.StartTime < x.EndTime && request.EndTime > x.StartTime, cancellationToken);

            if (overlap)
            {
                throw new Exception("This slot is overlap with another slot");
            }    
                 
            slot.Date = request.Date;
            slot.StartTime = request.StartTime;
            slot.EndTime = request.EndTime;

            await context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
