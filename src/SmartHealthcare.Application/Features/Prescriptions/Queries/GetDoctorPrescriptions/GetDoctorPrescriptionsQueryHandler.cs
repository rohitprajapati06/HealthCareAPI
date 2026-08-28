using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartHealthcare.Application.Common.Exceptions;
using SmartHealthcare.Application.Contracts.Identity;
using SmartHealthcare.Application.Contracts.Persistence;
using SmartHealthcare.Application.Features.Prescriptions.Responses;
using SmartHealthcare.Domain.Enums;

namespace SmartHealthcare.Application.Features.Prescriptions.Queries.GetDoctorPrescriptions
{
    public class GetDoctorPrescriptionsQueryHandler : IRequestHandler<GetDoctorPrescriptionsQuery, List<PrescriptionsResponse>>
    {
        private readonly IApplicationDbContext dbContext;
        private readonly ICurrentUserService currentUserService;

        public GetDoctorPrescriptionsQueryHandler(IApplicationDbContext dbContext, ICurrentUserService currentUserService)
        {
            this.dbContext = dbContext;
            this.currentUserService = currentUserService;
        }

        public async Task<List<PrescriptionsResponse>> Handle(GetDoctorPrescriptionsQuery request, CancellationToken cancellationToken)
        {
            // Controller already restricts this to Doctor/HospitalAdmin/SuperAdmin.
            // A Doctor caller may only see their own prescriptions.
            if (currentUserService.IsInRole(UserRoles.Doctor))
            {
                var ownDoctorProfileId = await dbContext.DoctorProfiles
                    .AsNoTracking()
                    .Where(d => d.UserId == currentUserService.UserId)
                    .Select(d => (Guid?)d.Id)
                    .FirstOrDefaultAsync(cancellationToken);

                if (ownDoctorProfileId == null || ownDoctorProfileId != request.DoctorId)
                {
                    throw new ForbiddenException("You are not allowed to view these prescriptions.");
                }
            }

            return await dbContext.Prescriptions
                .AsNoTracking()
                .Where(x => x.DoctorId == request.DoctorId)
                .Select(x => new PrescriptionsResponse
                {
                    Id = x.Id,
                    DoctorId = x.DoctorId,
                    DoctorName = x.DoctorProfile.User.FirstName + " " + x.DoctorProfile.User.LastName,
                    AppointmentId = x.AppointmentId,
                    Instructions = x.Instructions,
                    Medication = x.Medication,
                    CreatedAt = x.CreatedAt,
                }).ToListAsync(cancellationToken);
        }
    }
}