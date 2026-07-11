
namespace SmartHealthcare.Application.Features.AvailabilitySlots.Responses
{
    public class AvailabilitySlotResponse
    {
        public Guid Id { get; set; }

        public Guid DoctorId { get; set; }

        public DateOnly Date {  get; set; }

        public TimeOnly StartTime { get; set; }

        public TimeOnly EndTime{ get; set; }

        public bool IsBooked { get; set; }
    }
}
