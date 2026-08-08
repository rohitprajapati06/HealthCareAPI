

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

             TotalHospitals = await context.Hospitals.CountAsync(cancellationToken),
             TotalDoctors = await context.DoctorProfiles.CountAsync(cancellationToken),
             ApprovedDoctors = await context.DoctorProfiles.CountAsync(x => x.ApprovalStatus == DoctorApprovalStatus.Approved, cancellationToken),
             PendingDoctors = await context.DoctorProfiles.CountAsync(x => x.ApprovalStatus == DoctorApprovalStatus.Pending, cancellationToken),
             RejectedDoctors = await context.DoctorProfiles.CountAsync(x => x.ApprovalStatus == DoctorApprovalStatus.Rejected,cancellationToken),
             TotalPatients = await context.PatientProfiles.CountAsync(cancellationToken),
             TotalAppointments = await context.Appointments.CountAsync(cancellationToken),
             CompletedAppointments = await context.Appointments.CountAsync(x => x.Status == AppointmentStatus.Completed, cancellationToken),
             PendingAppointments = await context.Appointments.CountAsync(x => x.Status == AppointmentStatus.Pending,cancellationToken),
             CancelledAppointments = await context.Appointments.CountAsync(x => x.Status == AppointmentStatus.Cancelled,cancellationToken),
             TotalPrescriptions = await context.Prescriptions.CountAsync(cancellationToken),
             TotalMedicalRecords = await context.MedicalRecords.CountAsync(cancellationToken)

            };

            return response;
        }
    }
}
