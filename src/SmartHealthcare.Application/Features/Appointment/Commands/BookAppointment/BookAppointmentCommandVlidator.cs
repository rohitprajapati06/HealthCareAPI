using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartHealthcare.Application.Features.Appointment.Commands.BookAppointment
{
    public class BookAppointmentCommandVlidator:AbstractValidator<BookAppointmentCommand>
    {
        public BookAppointmentCommandVlidator()
        {
            RuleFor(x => x.DoctorId).NotEmpty();

            RuleFor(x => x.AppointmentDate).GreaterThan(DateTime.UtcNow).NotEmpty();

            RuleFor(x => x.Notes).MaximumLength(500);
        }
    }
}
