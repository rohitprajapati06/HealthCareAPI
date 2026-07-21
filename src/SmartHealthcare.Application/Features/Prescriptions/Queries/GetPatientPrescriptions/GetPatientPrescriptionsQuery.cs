using MediatR;
using SmartHealthcare.Application.Features.Prescriptions.Responses;


namespace SmartHealthcare.Application.Features.Prescriptions.Queries.GetPatientPrescriptions
{
    public record GetPatientPrescriptionsQuery(Guid PatientId) : IRequest<List<PrescriptionsResponses>>;
    
}
