

using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartHealthcare.Application.Contracts.Persistence;
using SmartHealthcare.Application.Features.Dashboard.Responses;
using SmartHealthcare.Domain.Enums;

namespace SmartHealthcare.Application.Features.Dashboard.Queries.GetDoctorDashboard
{
    public class GetDoctorDashboardQueryHandler : IRequestHandler<GetDoctorDashboardQuery,DoctorDashboardResponse>  
    {
        private readonly IApplicationDbContext context;

        public GetDoctorDashboardQueryHandler(IApplicationDbContext context)
        {
            this.context = context;
        }

        public async Task<DoctorDashboardResponse> Handle (GetDoctorDashboardQuery request , CancellationToken cancellationToken)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);

            var response = new DoctorDashboardResponse
            {
                todayAppointments = await context.Appointments.CountAsync(x => x.Id == request.DoctorId
                                && DateOnly.FromDateTime(x.AppointmentDate) == today, cancellationToken),

                pendingAppointments = await context.Appointments.CountAsync(x => x.Id == request.DoctorId
                                && x.Status == AppointmentStatus.Pending, cancellationToken),

                completedAppointment = await context.Appointments.CountAsync(x => x.Id == request.DoctorId
                                && x.Status == AppointmentStatus.Completed, cancellationToken),

                availableSlots = await context.AvailabilitySlots.CountAsync(x => x.Id == request.DoctorId
                                && !x.IsBooked, cancellationToken),

                prescriptionsCreated = await context.Prescriptions.CountAsync(x => x.Id == request.DoctorId,cancellationToken),


                PatientTreated = await context.Appointments.Where( x => x.DoctorId == request.DoctorId 
                            && x.Status == AppointmentStatus.Completed)
                            .Select(x => x.PatientId)
                            .Distinct()
                            .CountAsync(cancellationToken)







            };
             
            return response;
        }
    }
}
