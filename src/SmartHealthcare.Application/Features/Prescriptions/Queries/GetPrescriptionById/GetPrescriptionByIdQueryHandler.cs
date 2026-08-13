using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartHealthcare.Application.Common.Exceptions;
using SmartHealthcare.Application.Contracts.Persistence;
using SmartHealthcare.Application.Features.Prescriptions.Responses;

namespace SmartHealthcare.Application.Features.Prescriptions.Queries.GetPrescriptionById
{
    public class GetPrescriptionByIdQueryHandler : IRequestHandler<GetPrescriptionByIdQuery,PrescriptionsResponse>
    {
        private readonly IApplicationDbContext context;

        public GetPrescriptionByIdQueryHandler(IApplicationDbContext context)
        {
            this.context = context;
        }

        public async Task<PrescriptionsResponse> Handle(GetPrescriptionByIdQuery request, CancellationToken cancellationToken)
        {
            var prescription = await context.Prescriptions
                .AsNoTracking()
                .Where(x => x.Id == request.PrescriptionId)
                .Select(x => new PrescriptionsResponse
                {
                    Id = x.Id,
                    AppointmentId = x.AppointmentId,
                    DoctorId = x.DoctorId,
                    DoctorName = x.DoctorProfile.User.FirstName + " " + x.DoctorProfile.User.LastName,
                    Instructions = x.Instructions,
                    Medication = x.Medication,
                    CreatedAt = x.CreatedAt,

                }).FirstOrDefaultAsync(cancellationToken);

            if (prescription == null)
            {
                throw new NotFoundException("Prescription not found");
            }

            return prescription;
        }
            
    }
}
