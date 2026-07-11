using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartHealthcare.Application.Contracts.Persistence;
using SmartHealthcare.Domain.Entities;
using SmartHealthcare.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartHealthcare.Application.Features.AvailabilitySlots.Commands.CreateAvailabilitySlot
{
    public class CreateAvailabilitySlotCommandHandler:IRequestHandler<CreateAvailabilitySlot,Guid>
    {
        private readonly IApplicationDbContext context;

        public CreateAvailabilitySlotCommandHandler(IApplicationDbContext context)
        {
            this.context = context;
        }

        public async Task<Guid> Handle(CreateAvailabilitySlot request , CancellationToken cancellationToken)
        {
            var doctors = await context.DoctorProfiles.FirstOrDefaultAsync(x => x.Id == request.DoctorId,cancellationToken);

            if(doctors == null)
            {
                throw new Exception("Doctor not found");
            }

            if(doctors.ApprovalStatus != DoctorApprovalStatus.Approved)
            {
                throw new Exception("Doctor is Not Approved");
            }

            if(request.Date < DateOnly.FromDateTime(DateTime.Today))
            {
                throw new Exception("Cannot create slots for past dates.");
            }

            if(request.EndTime <= request.StartTime)
            {
                throw new Exception("End time must be greater than start time");
            }

            bool overlap = await context.AvailabilitySlots.AnyAsync(x => x.DoctorId == request.DoctorId && x.Date == request.Date &&
                            x.StartTime < request.EndTime && x.EndTime > request.StartTime, cancellationToken);

            if (overlap)
            {
                throw new Exception("Overlapping exists with the existing slot ");
            }

            var slot = new AvailabilitySlot
            {
                DoctorId = request.DoctorId,
                Date = request.Date,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                IsBooked = false

            };
                    
             await context.AvailabilitySlots.AddAsync(slot);
             await context.SaveChangesAsync(cancellationToken);

            return slot.Id; 
            
        }
    }
}
