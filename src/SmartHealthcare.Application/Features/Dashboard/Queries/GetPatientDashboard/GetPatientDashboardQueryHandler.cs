
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartHealthcare.Application.Contracts.Persistence;
using SmartHealthcare.Application.Features.Dashboard.Responses;
using SmartHealthcare.Domain.Enums;

namespace SmartHealthcare.Application.Features.Dashboard.Queries.GetPatientDashboard
{
    public class GetPatientDashboardQueryHandler : IRequestHandler<GetPatientDashboardQuery,PatientDashboardResponse>
    {
        private readonly IApplicationDbContext context;

        public GetPatientDashboardQueryHandler(IApplicationDbContext context)
        {
            this.context = context;
        }

        public async Task<PatientDashboardResponse> Handle(GetPatientDashboardQuery request , CancellationToken cancellationToken)
        {
            var patientId = await context.PatientProfiles.FirstOrDefaultAsync(x => x.Id == request.PatientId);

            if(patientId == null)
            {
                throw new ArgumentException("Patient Not Found");
            }

            var response = new PatientDashboardResponse
            {
                upcomingAppointment = await context.Appointments.CountAsync(x => x.Id == request.PatientId
                        && x.Status == AppointmentStatus.Confirmed, cancellationToken),

                completedAppointment = await context.Appointments.CountAsync(x => x.Id == request.PatientId
                        && x.Status == AppointmentStatus.Completed, cancellationToken),

                cancelledAppointment = await context.Appointments.CountAsync(x => x.Id == request.PatientId
                        && x.Status == AppointmentStatus.Cancelled, cancellationToken),

                medicalRecords = await context.MedicalRecords.CountAsync(x => x.Id == request.PatientId,cancellationToken),

                Prescriptions = await context.Prescriptions.CountAsync( x=> x.Id == request.PatientId,cancellationToken)
            };

            return response;
        }
    }
}
