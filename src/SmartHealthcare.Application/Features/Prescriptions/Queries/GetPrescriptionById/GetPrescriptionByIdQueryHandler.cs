using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartHealthcare.Application.Common.Exceptions;
using SmartHealthcare.Application.Contracts.Identity;
using SmartHealthcare.Application.Contracts.Persistence;
using SmartHealthcare.Application.Features.Prescriptions.Responses;
using SmartHealthcare.Domain.Enums;

namespace SmartHealthcare.Application.Features.Prescriptions.Queries.GetPrescriptionById
{
    public class GetPrescriptionByIdQueryHandler : IRequestHandler<GetPrescriptionByIdQuery, PrescriptionsResponse>
    {
        private readonly IApplicationDbContext context;
        private readonly ICurrentUserService currentUserService;

        public GetPrescriptionByIdQueryHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
        {
            this.context = context;
            this.currentUserService = currentUserService;
        }

        public async Task<PrescriptionsResponse> Handle(GetPrescriptionByIdQuery request, CancellationToken cancellationToken)
        {
            var prescription = await context.Prescriptions
                .AsNoTracking()
                .Where(x => x.Id == request.PrescriptionId)
                .Select(x => new
                {
                    Response = new PrescriptionsResponse
                    {
                        Id = x.Id,
                        AppointmentId = x.AppointmentId,
                        DoctorId = x.DoctorId,
                        DoctorName = x.DoctorProfile.User.FirstName + " " + x.DoctorProfile.User.LastName,
                        Instructions = x.Instructions,
                        Medication = x.Medication,
                        CreatedAt = x.CreatedAt,
                    },
                    x.Appointment.PatientId
                }).FirstOrDefaultAsync(cancellationToken);

            if (prescription == null)
            {
                throw new NotFoundException("Prescription not found");
            }

            if (currentUserService.IsInRole(UserRoles.Patient))
            {
                var ownPatientProfileId = await context.PatientProfiles
                    .AsNoTracking()
                    .Where(p => p.UserId == currentUserService.UserId)
                    .Select(p => (Guid?)p.Id)
                    .FirstOrDefaultAsync(cancellationToken);

                if (ownPatientProfileId == null || ownPatientProfileId != prescription.PatientId)
                {
                    throw new ForbiddenException("You are not allowed to view this prescription.");
                }
            }
            else if (currentUserService.IsInRole(UserRoles.Doctor))
            {
                var ownDoctorProfileId = await context.DoctorProfiles
                    .AsNoTracking()
                    .Where(d => d.UserId == currentUserService.UserId)
                    .Select(d => (Guid?)d.Id)
                    .FirstOrDefaultAsync(cancellationToken);

                if (ownDoctorProfileId == null || ownDoctorProfileId != prescription.Response.DoctorId)
                {
                    throw new ForbiddenException("You are not allowed to view this prescription.");
                }
            }

            return prescription.Response;
        }

    }
}