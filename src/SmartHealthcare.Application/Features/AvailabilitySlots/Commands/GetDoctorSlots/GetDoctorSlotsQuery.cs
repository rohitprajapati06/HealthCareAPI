using MediatR;
using SmartHealthcare.Application.Features.AvailabilitySlots.Responses;


namespace SmartHealthcare.Application.Features.AvailabilitySlots.Commands.GetDoctorSlots
{
    public class GetDoctorSlotsQuery:IRequest<List<AvailabilitySlotResponse>>
    {
    }
}
