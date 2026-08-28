using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartHealthcare.Application.Common.Exceptions;
using SmartHealthcare.Application.Contracts.Identity;
using SmartHealthcare.Application.Contracts.Persistence;
using SmartHealthcare.Application.Features.MedicalRecords.Responses;
using SmartHealthcare.Domain.Enums;

namespace SmartHealthcare.Application.Features.MedicalRecords.Queries.GetPatientMedicalRecords
{
    public class GetPatientMedicalRecordsQueryCommandHandler : IRequestHandler<GetPatientMedicalRecordsQuery,List<MedicalRecordResponse>>
    {
        private readonly IApplicationDbContext context;
        private readonly ICurrentUserService currentUserService;

        public GetPatientMedicalRecordsQueryCommandHandler(IApplicationDbContext context , ICurrentUserService currentUserService)
        {
            this.context = context;
            this.currentUserService = currentUserService;
        }

        public async Task<List<MedicalRecordResponse>> Handle(GetPatientMedicalRecordsQuery request , CancellationToken cancellationToken)
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
                    throw new ForbiddenException("You are not allowed to view these medical records.");
                }
            }         

            var medicalRecords = await context.MedicalRecords
                .Where(x => x.PatientId == request.PatientId)
                .Select(x => new MedicalRecordResponse
                {
                    Id = x.Id,
                    FileUrl = x.FileUrl,
                    FileName = x.FileName,
                    HospitalId = x.HospitalId,
                    HospitalName = x.Hospital.Name,
                    PatientId = x.PatientId,
                    PatientName = x.Patient.User.FirstName + " "+ x.Patient.User.LastName,
                    RecordType = x.RecordType,
                    CreatedAt = x.CreatedAt
                }).ToListAsync(cancellationToken);

            if(medicalRecords == null)
            {
                throw new NotFoundException("Medical records not found");
            }

            return medicalRecords;
        }
    }
}
