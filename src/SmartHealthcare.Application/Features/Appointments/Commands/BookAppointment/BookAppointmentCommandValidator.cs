using FluentValidation;

namespace SmartHealthcare.Application.Features.Appointments.Commands.BookAppointment
{
    public class BookAppointmentCommandValidator:AbstractValidator<BookAppointmentCommand>
    {
        public BookAppointmentCommandValidator()
        {
            RuleFor(x => x.DoctorId).NotEmpty();

            RuleFor(x => x.PatientId).NotEmpty();

            RuleFor(x => x.HospitalId).NotEmpty();

            RuleFor(x => x.AvailabilitySlotId).NotEmpty();

            RuleFor(x => x.Notes).MaximumLength(500);
        }
    }
}
