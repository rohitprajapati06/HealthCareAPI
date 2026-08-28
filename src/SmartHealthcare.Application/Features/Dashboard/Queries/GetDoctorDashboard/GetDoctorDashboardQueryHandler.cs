using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartHealthcare.Application.Common.Exceptions;
using SmartHealthcare.Application.Contracts.Identity;
using SmartHealthcare.Application.Contracts.Persistence;
using SmartHealthcare.Application.Features.Dashboard.Responses;
using SmartHealthcare.Domain.Enums;

namespace SmartHealthcare.Application.Features.Dashboard.Queries.GetDoctorDashboard
{
    public class GetDoctorDashboardQueryHandler : IRequestHandler<GetDoctorDashboardQuery, DoctorDashboardResponse>
    {
        private readonly IApplicationDbContext context;
        private readonly ICurrentUserService currentUserService;

        public GetDoctorDashboardQueryHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
        {
            this.context = context;
            this.currentUserService = currentUserService;
        }

        public async Task<DoctorDashboardResponse> Handle(GetDoctorDashboardQuery request, CancellationToken cancellationToken)
        {
            // Controller already restricts this to Doctor/HospitalAdmin/SuperAdmin.
            // A Doctor caller may only see their own dashboard.
            if (currentUserService.IsInRole(UserRoles.Doctor))
            {
                var ownDoctorProfileId = await context.DoctorProfiles
                    .AsNoTracking()
                    .Where(d => d.UserId == currentUserService.UserId)
                    .Select(d => (Guid?)d.Id)
                    .FirstOrDefaultAsync(cancellationToken);

                if (ownDoctorProfileId == null || ownDoctorProfileId != request.DoctorId)
                {
                    throw new ForbiddenException("You are not allowed to view this dashboard.");
                }
            }

            var today = DateOnly.FromDateTime(DateTime.Today);

            var response = new DoctorDashboardResponse
            {
                TodayAppointments = await context.Appointments.CountAsync(x => x.DoctorId == request.DoctorId
                                && DateOnly.FromDateTime(x.AppointmentDate) == today, cancellationToken),

                PendingAppointments = await context.Appointments.CountAsync(x => x.DoctorId == request.DoctorId
                                && x.Status == AppointmentStatus.Pending, cancellationToken),

                CompletedAppointments = await context.Appointments.CountAsync(x => x.DoctorId == request.DoctorId
                                && x.Status == AppointmentStatus.Completed, cancellationToken),

                AvailableSlots = await context.AvailabilitySlots.CountAsync(x => x.DoctorId == request.DoctorId
                                && !x.IsBooked, cancellationToken),

                PrescriptionsCreated = await context.Prescriptions.CountAsync(x => x.DoctorId == request.DoctorId, cancellationToken),


                PatientsTreated = await context.Appointments.Where(x => x.DoctorId == request.DoctorId
                            && x.Status == AppointmentStatus.Completed)
                            .Select(x => x.PatientId)
                            .Distinct()
                            .CountAsync(cancellationToken)

            };

            return response;
        }
    }
}