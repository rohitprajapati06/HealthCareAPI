

using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartHealthcare.Application.Contracts.Persistence;
using SmartHealthcare.Application.Features.Dashboard.Responses;
using SmartHealthcare.Domain.Enums;

namespace SmartHealthcare.Application.Features.Dashboard.Queries.GetAdminDashboard
{
    public class GetAdminDashboardQueryHandler: IRequestHandler<GetAdminDashboardQuery,AdminDashboardResponse>
    {
        private readonly IApplicationDbContext context;

        public GetAdminDashboardQueryHandler(IApplicationDbContext context)
        {
            this.context = context;
        }

        public async Task<AdminDashboardResponse> Handle (GetAdminDashboardQuery request , CancellationToken cancellationToken)
        {
            var response = new AdminDashboardResponse {

             totalHospitals = await context.Hospitals.CountAsync(cancellationToken),
             totalDoctors = await context.DoctorProfiles.CountAsync(cancellationToken),
             approvedDoctors = await context.DoctorProfiles.CountAsync(x => x.ApprovalStatus == DoctorApprovalStatus.Approved),
             pendingDoctors = await context.DoctorProfiles.CountAsync(x => x.ApprovalStatus == DoctorApprovalStatus.Pending),
             rejectedDoctors = await context.DoctorProfiles.CountAsync(x => x.ApprovalStatus == DoctorApprovalStatus.Rejected),
             totalPatients = await context.PatientProfiles.CountAsync(cancellationToken),
             totalAppointments = await context.Appointments.CountAsync(cancellationToken),
             completedAppointments = await context.Appointments.CountAsync(x => x.Status == AppointmentStatus.Completed),
             pendingAppointments = await context.Appointments.CountAsync(x => x.Status == AppointmentStatus.Pending),
             cancelledAppointments = await context.Appointments.CountAsync(x => x.Status == AppointmentStatus.Cancelled),
             totalPrescriptions = await context.Prescriptions.CountAsync(cancellationToken),
             totalMedicalRecords = await context.MedicalRecords.CountAsync(cancellationToken)

            };

            return response;
        }
    }
}
