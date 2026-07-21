

using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartHealthcare.Application.Contracts.Persistence;
using SmartHealthcare.Application.Features.Prescriptions.Responses;

namespace SmartHealthcare.Application.Features.Prescriptions.Queries.GetPatientPrescriptions
{
    public class GetPatientPrescriptionsQueryHandler : IRequestHandler<GetPatientPrescriptionsQuery,List<PrescriptionsResponses>>
    {
        private readonly IApplicationDbContext context;

        public GetPatientPrescriptionsQueryHandler(IApplicationDbContext context)
        {
            this.context = context;
        }

        public async Task<List<PrescriptionsResponses>> Handle(GetPatientPrescriptionsQuery request , CancellationToken cancellationToken)
        {
            return await context.Prescriptions
                .Include(p => p.DoctorProfile).ThenInclude(u => u.User)
                .Include(a => a.Appointment)
                .Where(x => x.Id == request.PatientId)
                .Select(x => new PrescriptionsResponses
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
