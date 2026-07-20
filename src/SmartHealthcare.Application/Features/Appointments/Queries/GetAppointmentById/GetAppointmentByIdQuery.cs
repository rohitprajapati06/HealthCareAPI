

using MediatR;
using SmartHealthcare.Application.Features.Appointments.Responses;

namespace SmartHealthcare.Application.Features.Appointments.Queries.GetAppointmentById
{
    public record GetAppointmentByIdQuery (Guid AppointmentId) : IRequest<AppointmentResponse>
    {

    }
}
