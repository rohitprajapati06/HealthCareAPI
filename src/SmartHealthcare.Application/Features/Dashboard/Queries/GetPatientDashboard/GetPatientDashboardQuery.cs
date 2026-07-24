

using MediatR;
using SmartHealthcare.Application.Features.Dashboard.Responses;

namespace SmartHealthcare.Application.Features.Dashboard.Queries.GetPatientDashboard
{
    public record GetPatientDashboardQuery(Guid PatientId) : IRequest<PatientDashboardResponse>;
    
}
