

using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartHealthcare.Application.Contracts.Persistence;
using SmartHealthcare.Application.Features.Prescriptions.Responses;

namespace SmartHealthcare.Application.Features.Prescriptions.Queries.GetDoctorPrescriptions
{
    public class GetDoctorPrescriptionsQueryHandler  : IRequestHandler<GetDoctorPrescriptionsQuery,List<PrescriptionsResponse>>
    {
        private readonly IApplicationDbContext dbContext;

        public GetDoctorPrescriptionsQueryHandler(IApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<List<PrescriptionsResponse>> Handle(GetDoctorPrescriptionsQuery request , CancellationToken cancellationToken) 
        {
            return await dbContext.Prescriptions
                .AsNoTracking()
                .Where(x => x.DoctorId == request.DoctorId)
                .Select(x => new PrescriptionsResponse
                {
                    Id = x.Id,
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
