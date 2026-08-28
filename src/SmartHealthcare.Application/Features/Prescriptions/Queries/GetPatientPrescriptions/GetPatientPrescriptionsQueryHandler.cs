using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartHealthcare.Application.Common.Exceptions;
using SmartHealthcare.Application.Contracts.Identity;
using SmartHealthcare.Application.Contracts.Persistence;
using SmartHealthcare.Application.Features.Prescriptions.Responses;
using SmartHealthcare.Domain.Enums;

namespace SmartHealthcare.Application.Features.Prescriptions.Queries.GetPatientPrescriptions
{
    public class GetPatientPrescriptionsQueryHandler : IRequestHandler<GetPatientPrescriptionsQuery, List<PrescriptionsResponse>>
    {
        private readonly IApplicationDbContext context;
        private readonly ICurrentUserService currentUserService;

        public GetPatientPrescriptionsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
        {
            this.context = context;
            this.currentUserService = currentUserService;
        }

        public async Task<List<PrescriptionsResponse>> Handle(GetPatientPrescriptionsQuery request, CancellationToken cancellationToken)
        {
            if (currentUserService.IsInRole(UserRoles.Patient))
            {
                var ownPatientProfileId = await context.PatientProfiles
                    .AsNoTracking()
                    .Where(p => p.UserId == currentUserService.UserId)
                    .Select(p => (Guid?)p.Id)
                    .FirstOrDefaultAsync(cancellationToken);

                if (ownPatientProfileId == null || ownPatientProfileId != request.PatientId)
                {
                    throw new ForbiddenException("You are not allowed to view these prescriptions.");
                }
            }

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