

using MediatR;
using SmartHealthcare.Application.Features.Doctors.Responses;

namespace SmartHealthcare.Application.Features.Doctors.Queries.GetDoctorsById
{
    public record GetDoctorsByIdQuery(Guid Id) : IRequest<DoctorResponse>;
    
}
