

using MediatR;
using Microsoft.EntityFrameworkCore;
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
            var records = await context.MedicalRecords
                .Include(x => x.Patient).ThenInclude(x => x.User)
                .Include(x => x.Hospital)
                .FirstOrDefaultAsync(x => x.Id == request.Id , cancellationToken);
                

            if(records == null)
            {
                throw new Exception("No Medical Record found");
            }

             return new MedicalRecordResponse
            {
                Id = records.Id,
                HospitalId = records.HospitalId ,
                HospitalName = records.Hospital.Name,
                PatientId = records.PatientId,
                PatientName = records.Patient.User.FirstName + " " + records.Patient.User.LastName,
                FileName = records.FileName ,
                FileUrl = records.FileUrl,   
                RecordType = records.RecordType ,
                CreatedAt = records.CreatedAt ,
            };

        }
    }
}
