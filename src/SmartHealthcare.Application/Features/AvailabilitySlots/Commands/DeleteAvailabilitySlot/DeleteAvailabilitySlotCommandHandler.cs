using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartHealthcare.Application.Common.Exceptions;
using SmartHealthcare.Application.Contracts.Identity;
using SmartHealthcare.Application.Contracts.Persistence;
using SmartHealthcare.Domain.Enums;
using System.Runtime.CompilerServices;

namespace SmartHealthcare.Application.Features.AvailabilitySlots.Commands.DeleteAvailabilitySlot
{
    public class DeleteAvailabilitySlotCommandHandler : IRequestHandler<DeleteAvailabilitySlotCommand>
    {
        private readonly IApplicationDbContext context;
        private readonly ILogger<DeleteAvailabilitySlotCommandHandler> logger;
        private readonly ICurrentUserService currentUserService;

        public DeleteAvailabilitySlotCommandHandler(IApplicationDbContext context, ILogger<DeleteAvailabilitySlotCommandHandler> logger, ICurrentUserService currentUserService)
        {
            this.context = context;
            this.logger = logger;
            this.currentUserService = currentUserService;
        }

        public async Task<Unit> Handle(DeleteAvailabilitySlotCommand request, CancellationToken cancellationToken)
        {
            var slot = await context.AvailabilitySlots.FirstOrDefaultAsync(x => x.Id == request.SlotId, cancellationToken);

            if (slot == null)
            {
                throw new NotFoundException("Availability slot not found.");
            }

            if (slot.DoctorId != request.DoctorId)
            {
                throw new ForbiddenException("You are not allowed to delete another doctor's availability slot.");
            }

            // The check above only confirms the slot matches the doctorId in the
            // route. Confirm the caller actually IS that doctor.
            if (currentUserService.IsInRole(UserRoles.Doctor))
            {
                var callingDoctor = await context.DoctorProfiles
                    .AsNoTracking()
                    .FirstOrDefaultAsync(d => d.Id == request.DoctorId, cancellationToken);

                if (callingDoctor == null || callingDoctor.UserId != currentUserService.UserId)
                {
                    throw new ForbiddenException("You are not allowed to delete another doctor's availability slot.");
                }
            }

            if (slot.IsBooked)
            {
                throw new ConflictException("Booked availability slots cannot be deleted.");
            }

            context.AvailabilitySlots.Remove(slot);
            await context.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Availability slot {SlotId} deleted for Doctor {DoctorId}.", slot.Id, slot.DoctorId);

            return Unit.Value;

        }
    }
}