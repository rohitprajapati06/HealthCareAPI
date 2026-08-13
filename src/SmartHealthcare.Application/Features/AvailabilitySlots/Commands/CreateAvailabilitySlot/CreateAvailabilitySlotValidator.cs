

using FluentValidation;

namespace SmartHealthcare.Application.Features.AvailabilitySlots.Commands.CreateAvailabilitySlot
{
    public class CreateAvailabilitySlotValidator:AbstractValidator<CreateAvailabilitySlot>
    {
        public CreateAvailabilitySlotValidator()
        {
            RuleFor(x => x.DoctorId).NotEmpty();

            RuleFor(x => x.Date).NotEmpty();

            RuleFor(x => x.StartTime).NotEmpty();

            RuleFor(x => x.EndTime).GreaterThan(x => x.StartTime).WithMessage("End time must be greater than start time.");
        }
    }
}
