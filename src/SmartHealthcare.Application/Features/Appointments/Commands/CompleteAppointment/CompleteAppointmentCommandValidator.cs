

using FluentValidation;
using MediatR;

namespace SmartHealthcare.Application.Features.Appointments.Commands.CompleteAppointment
{
    public class CompleteAppointmentCommandValidator : AbstractValidator<CompleteAppointmentCommand>
    {
        public CompleteAppointmentCommandValidator()
        {
            RuleFor(x => x.AppointmentId).NotEmpty();
        }
    }
}
