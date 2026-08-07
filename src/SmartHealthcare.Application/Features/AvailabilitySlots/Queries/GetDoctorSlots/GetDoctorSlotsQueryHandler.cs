using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartHealthcare.Application.Contracts.Persistence;
using SmartHealthcare.Application.Features.AvailabilitySlots.Responses;


namespace SmartHealthcare.Application.Features.AvailabilitySlots.Queries.GetDoctorSlots
{
    public class GetDoctorSlotsQueryHandler : IRequestHandler<GetDoctorSlotsQuery, List<AvailabilitySlotResponse>>
    {
        private readonly IApplicationDbContext context;

        public GetDoctorSlotsQueryHandler(IApplicationDbContext context)
        {
            this.context = context;
        }

        public async Task<List<AvailabilitySlotResponse>> Handle(GetDoctorSlotsQuery request, CancellationToken cancellationToken)
        {
            return await context.AvailabilitySlots
                .AsNoTracking()
                .Where(x => x.DoctorId == request.DoctorId && x.Date == DateOnly.FromDateTime(DateTime.Today))
                .OrderBy(x => x.Date)
                .ThenBy(x => x.StartTime)
                .Select(x => new AvailabilitySlotResponse
                {
                    Id = x.Id,
                    DoctorId = x.DoctorId,
                    StartTime = x.StartTime,
                    EndTime = x.EndTime,
                    Date = x.Date,
                    IsBooked = x.IsBooked,
                })
                .ToListAsync(cancellationToken);
                

        }
    }
}