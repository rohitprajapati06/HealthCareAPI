using MediatR;
using SmartHealthcare.Application.Features.Dashboard.Responses;


namespace SmartHealthcare.Application.Features.Dashboard.Queries.GetDoctorDashboard
{
    public record GetDoctorDashboardQuery (Guid DoctorId ): IRequest<DoctorDashboardResponse>;
    
    
}
