

using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartHealthcare.Application.Common.Exceptions;
using SmartHealthcare.Application.Contracts.Persistence;
using SmartHealthcare.Application.Features.MedicalRecords.Responses;

namespace SmartHealthcare.Application.Features.MedicalRecords.Queries.GetMedicalRecordById
{
    public class GetMedicalRecordByIdQueryHandler : IRequestHandler<GetMedicalRecordById,MedicalRecordResponse>
    {
        private readonly IApplicationDbContext context;

        public GetMedicalRecordByIdQueryHandler(IApplicationDbContext context)
        {
            this.context = context;
        }

        public async Task<MedicalRecordResponse> Handle(GetMedicalRecordById request , CancellationToken cancellationToken)
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

            return record;
            

        }
    }
}
