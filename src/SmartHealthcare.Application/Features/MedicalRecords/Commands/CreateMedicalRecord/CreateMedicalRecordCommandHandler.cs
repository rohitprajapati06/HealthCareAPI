

using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartHealthcare.Application.Contracts.Persistence;
using SmartHealthcare.Domain.Entities;

namespace SmartHealthcare.Application.Features.MedicalRecords.Commands.CreateMedicalRecord
{
    public class CreateMedicalRecordCommandHandler : IRequestHandler<CreateMedicalRecordCommand,Guid>
    {
        private readonly IApplicationDbContext context;

        public CreateMedicalRecordCommandHandler(IApplicationDbContext context)
        {
            this.context = context;
        }

        public async Task<Guid> Handle(CreateMedicalRecordCommand request , CancellationToken cancellationToken)
        {
            var patient = await context.PatientProfiles.FirstOrDefaultAsync(x => x.Id == request.Id,cancellationToken);

            if(patient == null) {
                throw new Exception("Patient not found");
            }

            var hospital = await context.Hospitals.FirstOrDefaultAsync(x => x.Id == request.HospitalId,cancellationToken);

            if(hospital == null)
            {
                throw new Exception("Hospitals not found");
            }

            var medicalrecord = new MedicalRecord
            {
                Id = request.Id,
                HospitalId = request.HospitalId,
                FileName = request.FileName,
                FileUrl = request.FileUrl,
                RecordType = request.RecordType
            };
            await context.MedicalRecords.AddAsync(medicalrecord);
            await context.SaveChangesAsync(cancellationToken);

            return request.Id;
        }
    }
}
