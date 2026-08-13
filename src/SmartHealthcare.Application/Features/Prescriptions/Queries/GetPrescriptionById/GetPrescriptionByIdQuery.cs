using MediatR;
using SmartHealthcare.Application.Features.Prescriptions.Responses;

namespace SmartHealthcare.Application.Features.Prescriptions.Queries.GetPrescriptionById
{
    public record GetPrescriptionByIdQuery(Guid PrescriptionId) : IRequest<PrescriptionsResponse>;

}