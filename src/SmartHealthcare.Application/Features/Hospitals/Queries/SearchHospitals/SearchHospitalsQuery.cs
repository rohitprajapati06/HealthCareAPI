

using MediatR;
using SmartHealthcare.Application.Features.Hospitals.Responses;

namespace SmartHealthcare.Application.Features.Hospitals.Queries.SearchHospitals
{
    public record SearchHospitalsQuery(

        string? Name,
        string? City,
        string? State
    ) : IRequest<List<HospitalResponse>>;

    
}
