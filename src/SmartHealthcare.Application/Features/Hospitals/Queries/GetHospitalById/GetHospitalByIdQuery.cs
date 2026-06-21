

using MediatR;
using SmartHealthcare.Application.Features.Hospitals.Responses;

namespace SmartHealthcare.Application.Features.Hospitals.Queries.GetHospitalById
{
    public record GetHospitalByIdQuery(Guid Id):IRequest<HospitalResponse>
    {
    }
}
