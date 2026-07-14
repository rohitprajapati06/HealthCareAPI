
using FluentValidation;

namespace SmartHealthcare.Application.Features.AvailabilitySlots.Commands.UpdateAvailabilitySlot
{
    public class UpdateAvailabilitySlotValidator:AbstractValidator<UpdateAvailabilitySlotCommand>
    {
        public UpdateAvailabilitySlotValidator()
        {
            RuleFor(x => x.SlotId).NotEmpty();
            RuleFor(x => x.DoctorId).NotEmpty();
            RuleFor(x => x.Date).NotEmpty();
            RuleFor(x => x)
                .Must(x => x.EndTime > x.StartTime)
                .WithMessage("End time must be greater than the start time");

        }
    }
}
