using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartHealthcare.Application.Common.Exceptions;
using SmartHealthcare.Application.Contracts.Identity;
using SmartHealthcare.Application.Contracts.Persistence;
using SmartHealthcare.Domain.Entities;
using SmartHealthcare.Domain.Enums;


namespace SmartHealthcare.Application.Features.AvailabilitySlots.Commands.CreateAvailabilitySlot
{
    public class CreateAvailabilitySlotCommandHandler : IRequestHandler<CreateAvailabilitySlot, Guid>
    {
        private readonly IApplicationDbContext context;
        private readonly ILogger<CreateAvailabilitySlotCommandHandler> logger;
        private readonly ICurrentUserService currentUserService;

        public CreateAvailabilitySlotCommandHandler(IApplicationDbContext context, ILogger<CreateAvailabilitySlotCommandHandler> logger, ICurrentUserService currentUserService)
        {
            this.context = context;
            this.logger = logger;
            this.currentUserService = currentUserService;
        }

        public async Task<Guid> Handle(CreateAvailabilitySlot request, CancellationToken cancellationToken)
        {
            var doctor = await context.DoctorProfiles.FirstOrDefaultAsync(x => x.Id == request.DoctorId, cancellationToken);

            if (doctor == null)
            {
                throw new NotFoundException("Doctor not found");
            }

            // A Doctor caller may only create slots for themselves.
            if (currentUserService.IsInRole(UserRoles.Doctor) && doctor.UserId != currentUserService.UserId)
            {
                throw new ForbiddenException("You cannot create availability slots for another doctor.");
            }

            if (doctor.ApprovalStatus != DoctorApprovalStatus.Approved)
            {
                throw new ForbiddenException("Doctor is Not Approved");
            }

            if (request.Date < DateOnly.FromDateTime(DateTime.Today))
            {
                throw new BadRequestException("Cannot create slots for past dates.");
            }

            if (request.EndTime <= request.StartTime)
            {
                throw new BadRequestException("End time must be greater than start time");
            }

            bool overlap = await context.AvailabilitySlots.AnyAsync(x => x.DoctorId == request.DoctorId && x.Date == request.Date &&
                            x.StartTime < request.EndTime && x.EndTime > request.StartTime, cancellationToken);

            if (overlap)
            {
                logger.LogError("Slot has been overlap");
                throw new ConflictException("Overlapping exists with the existing slot ");

            }

            var slot = new AvailabilitySlot
            {
                DoctorId = request.DoctorId,
                Date = request.Date,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                IsBooked = false

            };

            await context.AvailabilitySlots.AddAsync(slot, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);

            logger.LogInformation(
                "Availability slot {SlotId} created for Doctor {DoctorId}.",
                slot.Id,
                slot.DoctorId);


            return slot.Id;

        }
    }
}