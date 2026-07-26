

using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartHealthcare.Application.Common.Exceptions;
using SmartHealthcare.Application.Contracts.Persistence;
using SmartHealthcare.Application.Features.Prescriptions.Responses;
using SmartHealthcare.Domain.Entities;

namespace SmartHealthcare.Application.Features.Prescriptions.Queries.GetPrescriptionById
{
    public class GetPrescriptionByIdQueryHandler : IRequestHandler<GetPrescriptionByIdQuery,PrescriptionsResponses>
    {
        private readonly IApplicationDbContext context;

        public GetPrescriptionByIdQueryHandler(IApplicationDbContext context)
        {
            this.context = context;
        }

        public async Task<PrescriptionsResponses> Handle(GetPrescriptionByIdQuery request , CancellationToken cancellationToken)
        {
            var prescription = await context.Prescriptions
                .Include(x => x.DoctorProfile).ThenInclude(u => u.User)
                .FirstOrDefaultAsync(x => x.Id == request.PrescriptionId);

            if(prescription == null)
            {
                throw new NotFoundException("Prescription not found");
            }

            return new PrescriptionsResponses
            {
                AppointmentId = request.PrescriptionId,
                DoctorId = prescription.DoctorId,
                DoctorName = prescription.DoctorProfile.User.FirstName + " " + prescription.DoctorProfile.User.LastName,
                Instructions = prescription.Instructions,
                Medication = prescription.Medication,
                CreatedAt = prescription.CreatedAt,
            };
        }
    }
}
