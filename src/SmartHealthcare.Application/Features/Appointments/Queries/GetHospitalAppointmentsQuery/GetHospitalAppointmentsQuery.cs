
using MediatR;
using SmartHealthcare.Application.Features.Appointments.Responses;

namespace SmartHealthcare.Application.Features.Appointments.Queries.GetHospitalAppointmentsQuery
{
    public record GetHospitalAppointmentsQuery(Guid HospitalId) : IRequest<List<AppointmentResponse>>;
    
}
