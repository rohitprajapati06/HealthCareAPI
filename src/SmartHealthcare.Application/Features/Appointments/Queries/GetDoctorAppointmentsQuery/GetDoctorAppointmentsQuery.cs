

using MediatR;
using SmartHealthcare.Application.Features.Appointments.Responses;

namespace SmartHealthcare.Application.Features.Appointments.Queries.GetDoctorAppointmentsQuery
{
    public record GetDoctorAppointmentsQuery(Guid DoctorId) : IRequest<List<AppointmentResponse>>;
    
}
