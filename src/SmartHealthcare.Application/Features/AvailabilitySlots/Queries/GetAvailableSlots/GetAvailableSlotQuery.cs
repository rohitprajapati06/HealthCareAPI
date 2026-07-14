

using MediatR;
using SmartHealthcare.Application.Features.AvailabilitySlots.Responses;

namespace SmartHealthcare.Application.Features.AvailabilitySlots.Queries.GetAvailableSlots
{
    public record GetAvailableSlotQuery( Guid DoctorId ) : IRequest<List<AvailabilitySlotResponse>>;
    
}
