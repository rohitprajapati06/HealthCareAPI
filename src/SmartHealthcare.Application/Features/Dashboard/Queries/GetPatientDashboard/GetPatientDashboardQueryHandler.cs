
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartHealthcare.Application.Common.Exceptions;
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
            var patientId = await context.PatientProfiles.AnyAsync(x => x.Id == request.PatientId,cancellationToken);

            if(!patientId)
            {
                throw new NotFoundException("Patient Not Found");
            }

            var response = new PatientDashboardResponse
            {
                UpcomingAppointment = await context.Appointments.CountAsync(x => x.PatientId == request.PatientId 
                        && x.AppointmentDate > DateTime.Now 
                        && x.Status == AppointmentStatus.Confirmed, cancellationToken),

                CompletedAppointment = await context.Appointments.CountAsync(x => x.PatientId == request.PatientId
                        && x.Status == AppointmentStatus.Completed, cancellationToken),

                CancelledAppointment = await context.Appointments.CountAsync(x => x.PatientId == request.PatientId
                        && x.Status == AppointmentStatus.Cancelled, cancellationToken),

                MedicalRecords = await context.MedicalRecords.CountAsync(x => x.PatientId == request.PatientId,cancellationToken),

                Prescriptions = await context.Prescriptions.CountAsync( x=> x.Appointment.PatientId == request.PatientId,cancellationToken)
            };

            return response;
        }
    }
}
