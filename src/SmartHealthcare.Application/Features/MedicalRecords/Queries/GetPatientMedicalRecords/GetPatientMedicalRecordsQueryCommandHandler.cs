using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartHealthcare.Application.Contracts.Persistence;
using SmartHealthcare.Application.Features.MedicalRecords.Responses;

namespace SmartHealthcare.Application.Features.MedicalRecords.Queries.GetPatientMedicalRecords
{
    public class GetPatientMedicalRecordsQueryCommandHandler : IRequestHandler<GetPatientMedicalRecordsQuery,List<MedicalRecordResponse>>
    {
        private readonly IApplicationDbContext context;

        public GetPatientMedicalRecordsQueryCommandHandler(IApplicationDbContext context)
        {
            this.context = context;
        }

        public async Task<List<MedicalRecordResponse>> Handle(GetPatientMedicalRecordsQuery request , CancellationToken cancellationToken)
        {
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

            return medicalRecords;
        }
    }
}
