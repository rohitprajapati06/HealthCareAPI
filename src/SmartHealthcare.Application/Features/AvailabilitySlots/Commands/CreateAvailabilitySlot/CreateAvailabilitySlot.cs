
using MediatR;

namespace SmartHealthcare.Application.Features.AvailabilitySlots.Commands.CreateAvailabilitySlot
{
    public class CreateAvailabilitySlot : IRequest<Guid>
    {
        public Guid DoctorId { get; set; }
    
        public DateOnly Date {  get; set; }

        public TimeOnly StartTime { get; set; }

        public TimeOnly EndTime { get; set; }
    }
}
