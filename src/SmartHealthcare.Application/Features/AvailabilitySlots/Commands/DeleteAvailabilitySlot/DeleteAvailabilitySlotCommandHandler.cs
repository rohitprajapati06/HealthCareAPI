
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartHealthcare.Application.Contracts.Persistence;
using System.Runtime.CompilerServices;

namespace SmartHealthcare.Application.Features.AvailabilitySlots.Commands.DeleteAvailabilitySlot
{
    public class DeleteAvailabilitySlotCommandHandler : IRequestHandler<DeleteAvailabilitySlotCommand>
    {
        private readonly IApplicationDbContext context;

        public DeleteAvailabilitySlotCommandHandler(IApplicationDbContext context)
        {
            this.context = context;
        }

        public async Task<Unit> Handle(DeleteAvailabilitySlotCommand request , CancellationToken cancellationToken)
        {
            var slotId = await context.AvailabilitySlots.FirstOrDefaultAsync(x => x.Id == request.SlotId,cancellationToken);

            if(slotId == null)
            {
                throw new Exception("There is no slot exist");
            }

            if(slotId.DoctorId != request.DoctorId)
            {
                throw new Exception("You cannot delete the other doctors appointment");
            }

            if (slotId.IsBooked)
            {
                throw new Exception("You cannot delete the booked slots");
            }

             context.AvailabilitySlots.Remove(slotId);
            await context.SaveChangesAsync(cancellationToken);

            return Unit.Value;

        }
    }
}
