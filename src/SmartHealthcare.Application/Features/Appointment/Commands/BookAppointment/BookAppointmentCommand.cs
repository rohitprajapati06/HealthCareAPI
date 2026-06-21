using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartHealthcare.Application.Features.Appointment.Commands.BookAppointment
{
    public class BookAppointmentCommand:IRequest<Guid>
    {
        public Guid DoctorId { get; set; }

        public DateTime AppointmentDate { get; set; }

        public string? Notes { get; set; } = string.Empty;
    }
}
