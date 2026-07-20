using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartHealthcare.Application.Features.Appointments.Commands.CancelAppointment
{
    public class CancelAppointmentCommand : IRequest
    {
        public Guid AppointmentId { get; set; }
    }
}
