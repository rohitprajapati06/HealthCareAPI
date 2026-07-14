

using MediatR;

namespace SmartHealthcare.Application.Features.AvailabilitySlots.Commands.UpdateAvailabilitySlot;

public class UpdateAvailabilitySlotCommand : IRequest
{
    public Guid SlotId { get; set; }    

    public Guid DoctorId { get; set; }

    public DateOnly Date {  get; set; }

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }

}
