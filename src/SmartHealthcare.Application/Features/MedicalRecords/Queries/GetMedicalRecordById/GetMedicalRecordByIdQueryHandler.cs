using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartHealthcare.Application.Common.Exceptions;
using SmartHealthcare.Application.Contracts.Identity;
using SmartHealthcare.Application.Contracts.Persistence;
using SmartHealthcare.Application.Features.MedicalRecords.Responses;
using SmartHealthcare.Domain.Enums;

namespace SmartHealthcare.Application.Features.MedicalRecords.Queries.GetMedicalRecordById
{
    public class GetMedicalRecordByIdQueryHandler : IRequestHandler<GetMedicalRecordById, MedicalRecordResponse>
    {
        private readonly IApplicationDbContext context;
        private readonly ICurrentUserService currentUserService;

        public GetMedicalRecordByIdQueryHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
        {
            this.context = context;
            this.currentUserService = currentUserService;
        }

        public async Task<MedicalRecordResponse> Handle(GetMedicalRecordById request, CancellationToken cancellationToken)
        {
            var record = await context.MedicalRecords
                .AsNoTracking()
                .Where(x => x.Id == request.Id)
                .Select(x => new MedicalRecordResponse
                {
                    Id = x.Id,
                    HospitalId = x.HospitalId,
                    HospitalName = x.Hospital.Name,
                    PatientId = x.PatientId,
                    PatientName = x.Patient.User.FirstName + " " + x.Patient.User.LastName,
                    FileName = x.FileName,
                    FileUrl = x.FileUrl,
                    RecordType = x.RecordType,
                    CreatedAt = x.CreatedAt,
                }).FirstOrDefaultAsync(cancellationToken);


            if(record == null)
            {
                throw new NotFoundException("No Medical Record found");
            }

            if (currentUserService.IsInRole(UserRoles.Patient))
            {
                var ownPatientProfileId = await context.PatientProfiles
                    .AsNoTracking()
                    .Where(p => p.UserId == currentUserService.UserId)
                    .Select(p => (Guid?)p.Id)
                    .FirstOrDefaultAsync(cancellationToken);

                if (ownPatientProfileId == null || ownPatientProfileId != record.PatientId)
                {
                    throw new ForbiddenException("You are not allowed to view this medical record.");
                }
            }

            return record;


        }
    }
}