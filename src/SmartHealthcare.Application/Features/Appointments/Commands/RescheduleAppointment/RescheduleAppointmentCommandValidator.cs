using FluentValidation;


namespace SmartHealthcare.Application.Features.Appointments.Commands.RescheduleAppointment
{
    public class RescheduleAppointmentCommandValidator : AbstractValidator<RescheduleAppointmentCommand>
    {
        public RescheduleAppointmentCommandValidator()
        {
            RuleFor(x => x.AppointmentId).NotEmpty();
            RuleFor(x => x.AvailabilitySlotId).NotEmpty();
        }
    }
}
