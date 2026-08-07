
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartHealthcare.Application.Contracts.Persistence;
using SmartHealthcare.Application.Features.AvailabilitySlots.Responses;

namespace SmartHealthcare.Application.Features.AvailabilitySlots.Queries.GetAvailableSlots
{
    public class GetAvailableSlotQueryHandler : IRequestHandler<GetAvailableSlotQuery,List<AvailabilitySlotResponse>>
    {
        private readonly IApplicationDbContext context;

        public GetAvailableSlotQueryHandler(IApplicationDbContext context)
        {
            this.context = context;
        }

        public async Task<List<AvailabilitySlotResponse>> Handle(GetAvailableSlotQuery request , CancellationToken cancellationToken)
        {
             return await context.AvailabilitySlots
                .AsNoTracking()
                .Where(x => x.DoctorId == request.DoctorId && !x.IsBooked)
                .OrderBy(x => x.Date)
                .ThenBy(x => x.StartTime)
                .Select(x => new AvailabilitySlotResponse
                {
                    Id = x.Id,
                    DoctorId = x.DoctorId,
                    Date = x.Date,
                    StartTime = x.StartTime,
                    EndTime = x.EndTime,
                    IsBooked = x.IsBooked,
                })
                .ToListAsync(cancellationToken);
        }
    }
}
