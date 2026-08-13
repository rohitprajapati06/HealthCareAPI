

using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartHealthcare.Application.Contracts.Persistence;
using SmartHealthcare.Application.Features.Prescriptions.Responses;

namespace SmartHealthcare.Application.Features.Prescriptions.Queries.GetPatientPrescriptions
{
    public class GetPatientPrescriptionsQueryHandler : IRequestHandler<GetPatientPrescriptionsQuery,List<PrescriptionsResponse>>
    {
        private readonly IApplicationDbContext context;

        public GetPatientPrescriptionsQueryHandler(IApplicationDbContext context)
        {
            this.context = context;
        }

        public async Task<List<PrescriptionsResponse>> Handle(GetPatientPrescriptionsQuery request , CancellationToken cancellationToken)
        {
            return await context.Prescriptions
                .AsNoTracking()
                .Where(x => x.Appointment.PatientId == request.PatientId)
                .Select(x => new PrescriptionsResponse
                {
                    Id = x.Id,
                    AppointmentId = x.AppointmentId,
                    DoctorId = x.DoctorId,
                    DoctorName = x.DoctorProfile.User.FirstName + " " + x.DoctorProfile.User.LastName,
                    Instructions = x.Instructions,
                    Medication = x.Medication,
                    CreatedAt = x.CreatedAt            
                }).ToListAsync(cancellationToken);

            

        }
    }
}
