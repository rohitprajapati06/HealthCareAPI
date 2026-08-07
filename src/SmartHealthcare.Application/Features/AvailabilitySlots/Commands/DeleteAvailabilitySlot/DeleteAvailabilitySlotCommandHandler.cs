
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartHealthcare.Application.Common.Exceptions;
using SmartHealthcare.Application.Contracts.Persistence;
using System.Runtime.CompilerServices;

namespace SmartHealthcare.Application.Features.AvailabilitySlots.Commands.DeleteAvailabilitySlot
{
    public class DeleteAvailabilitySlotCommandHandler : IRequestHandler<DeleteAvailabilitySlotCommand>
    {
        private readonly IApplicationDbContext context;
        private readonly ILogger<DeleteAvailabilitySlotCommandHandler> logger;

        public DeleteAvailabilitySlotCommandHandler(IApplicationDbContext context ,ILogger<DeleteAvailabilitySlotCommandHandler> logger)
        {
            this.context = context;
            this.logger = logger;
        }

        public async Task<Unit> Handle(DeleteAvailabilitySlotCommand request , CancellationToken cancellationToken)
        {
            var slot = await context.AvailabilitySlots.FirstOrDefaultAsync(x => x.Id == request.SlotId,cancellationToken);

            if(slot == null)
            {
                throw new NotFoundException("Availability slot not found.");
            }

            if(slot.DoctorId != request.DoctorId)
            {
                throw new ForbiddenException("You are not allowed to delete another doctor's availability slot.");
            }

            if (slot.IsBooked)
            {
                throw new ConflictException("Booked availability slots cannot be deleted.");
            }

             context.AvailabilitySlots.Remove(slot);
            await context.SaveChangesAsync(cancellationToken);

            logger.LogInformation( "Availability slot {SlotId} deleted for Doctor {DoctorId}.",slot.Id,slot.DoctorId);

            return Unit.Value;

        }
    }
}
