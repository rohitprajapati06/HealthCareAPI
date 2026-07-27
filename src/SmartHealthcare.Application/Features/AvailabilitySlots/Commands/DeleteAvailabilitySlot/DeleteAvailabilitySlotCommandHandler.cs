
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
        private readonly ILogger logger;

        public DeleteAvailabilitySlotCommandHandler(IApplicationDbContext context ,ILogger logger)
        {
            this.context = context;
            this.logger = logger;
        }

        public async Task<Unit> Handle(DeleteAvailabilitySlotCommand request , CancellationToken cancellationToken)
        {
            var slotId = await context.AvailabilitySlots.FirstOrDefaultAsync(x => x.Id == request.SlotId,cancellationToken);

            if(slotId == null)
            {
                throw new NotFoundException("There is no slot exist");
            }

            if(slotId.DoctorId != request.DoctorId)
            {
                throw new ConflictException("You cannot delete the other doctors appointment");
            }

            if (slotId.IsBooked)
            {
                throw new ConflictException("You cannot delete the booked slots");
            }

             context.AvailabilitySlots.Remove(slotId);
            await context.SaveChangesAsync(cancellationToken);

            logger.LogInformation($"Available slot has been deleted");

            return Unit.Value;

        }
    }
}
