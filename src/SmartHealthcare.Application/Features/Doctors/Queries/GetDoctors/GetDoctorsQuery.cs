

using MediatR;
using SmartHealthcare.Application.Features.Doctors.Responses;

namespace SmartHealthcare.Application.Features.Doctors.Queries.GetDoctors
{
    public record GetDoctorsQuery() : IRequest<List<DoctorResponse>>;
    
}
