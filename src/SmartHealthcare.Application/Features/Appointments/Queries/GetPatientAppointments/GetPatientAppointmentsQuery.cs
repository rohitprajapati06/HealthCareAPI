

using MediatR;
using SmartHealthcare.Application.Features.Appointments.Responses;

namespace SmartHealthcare.Application.Features.Appointments.Queries.GetPatientAppointments
{
    public record GetPatientAppointmentsQuery(Guid PatientId) : IRequest<List<AppointmentResponse>>;
    
}
