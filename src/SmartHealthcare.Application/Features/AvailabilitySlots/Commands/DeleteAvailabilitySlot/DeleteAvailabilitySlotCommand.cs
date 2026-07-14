

using MediatR;
using SmartHealthcare.Application.Features.AvailabilitySlots.Responses;

namespace SmartHealthcare.Application.Features.AvailabilitySlots.Commands.DeleteAvailabilitySlot
{
    public class DeleteAvailabilitySlotCommand : IRequest<Unit>
    {
        public Guid SlotId { get; set; }

        public Guid DoctorId { get; set; }
    }
}
