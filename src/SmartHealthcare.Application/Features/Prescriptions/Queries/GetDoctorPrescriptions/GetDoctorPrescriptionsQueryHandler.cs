

using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartHealthcare.Application.Contracts.Persistence;
using SmartHealthcare.Application.Features.Prescriptions.Responses;

namespace SmartHealthcare.Application.Features.Prescriptions.Queries.GetDoctorPrescriptions
{
    public class GetDoctorPrescriptionsQueryHandler  : IRequestHandler<GetDoctorPrescriptionsQuery,List<PrescriptionsResponses>>
    {
        private readonly IApplicationDbContext dbContext;

        public GetDoctorPrescriptionsQueryHandler(IApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<List<PrescriptionsResponses>> Handle(GetDoctorPrescriptionsQuery request , CancellationToken cancellationToken) 
        {
            return await dbContext.Prescriptions
                .Include(d => d.DoctorProfile).ThenInclude(u => u.User)
                .Include(a => a.Appointment)
                .Where(x => x.DoctorId == request.DoctorId)
                .Select(x => new PrescriptionsResponses
                {
                    Id = x.DoctorId,
                    DoctorId = x.DoctorId,
                    DoctorName = x.DoctorProfile.User.FirstName + " "+ x.DoctorProfile.User.LastName,
                    AppointmentId = x.AppointmentId,
                    Instructions = x.Instructions,
                    Medication = x.Medication,
                    CreatedAt = x.CreatedAt,
                }).ToListAsync(cancellationToken);
        }
    }
}
