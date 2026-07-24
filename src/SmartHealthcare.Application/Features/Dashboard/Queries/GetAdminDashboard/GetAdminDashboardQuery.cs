

using MediatR;
using SmartHealthcare.Application.Features.Dashboard.Responses;

namespace SmartHealthcare.Application.Features.Dashboard.Queries.GetAdminDashboard
{
    public record GetAdminDashboardQuery : IRequest<AdminDashboardResponse>;
    
    
}
