

using MediatR;
using SmartHealthcare.Application.Features.Hospitals.Responses;

namespace SmartHealthcare.Application.Features.Hospitals.Queries.GetHospitals;

public record GetHospitalsQuery() : IRequest<List<HospitalResponse>>;

