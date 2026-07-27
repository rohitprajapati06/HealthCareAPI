

using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartHealthcare.Application.Common.Exceptions;
using SmartHealthcare.Application.Contracts.Persistence;

namespace SmartHealthcare.Application.Features.AvailabilitySlots.Commands.UpdateAvailabilitySlot
{
    public class UpdateAvailabilitySlotCommandHandler : IRequestHandler<UpdateAvailabilitySlotCommand>
    {
        private readonly IApplicationDbContext context;
        private readonly ILogger logger;

        public UpdateAvailabilitySlotCommandHandler(IApplicationDbContext context , ILogger logger)
        {
            this.context = context;
            this.logger = logger;
        }

        public async Task<Unit> Handle(UpdateAvailabilitySlotCommand request, CancellationToken cancellationToken)
        {
            var slot = await context.AvailabilitySlots
               .FirstOrDefaultAsync(x => x.Id == request.SlotId, cancellationToken);

            if(slot == null)
            {
                throw new NotFoundException("There is no slots booked");
            }

            if(slot.DoctorId != request.DoctorId)
            {
                throw new ForbiddenException("You cannot update the another doctors slots ");
            }

            if (slot.IsBooked)
            {
                throw new ConflictException("There is no slot available");
            }

            if(slot.Date < DateOnly.FromDateTime(DateTime.Today))
            {
                throw new ConflictException("You cannot update the past date slots");
            }

            if(request.EndTime <= request.StartTime)
            {
                throw new ConflictException("End time must be greater than the start time");
            }

            var overlap = await context.AvailabilitySlots
                .AnyAsync(x => x.Id != slot.Id && x.DoctorId == request.DoctorId && x.Date == request.Date
                     && request.StartTime < x.EndTime && request.EndTime > x.StartTime, cancellationToken);

            if (overlap)
            {
                throw new ConflictException("This slot is overlap with another slot");
            }    
                 
            slot.Date = request.Date;
            slot.StartTime = request.StartTime;
            slot.EndTime = request.EndTime;

            await context.SaveChangesAsync(cancellationToken);

            logger.LogInformation($"Available slot has been updated {slot.Id}");

            return Unit.Value;
        }
    }
}
