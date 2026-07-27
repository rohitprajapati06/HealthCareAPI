

using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartHealthcare.Application.Common.Exceptions;
using SmartHealthcare.Application.Common.Interface;
using SmartHealthcare.Application.Contracts.Persistence;
using SmartHealthcare.Domain.Entities;

namespace SmartHealthcare.Application.Features.MedicalRecords.Commands.CreateMedicalRecord
{
    public class CreateMedicalRecordCommandHandler : IRequestHandler<CreateMedicalRecordCommand,Guid>
    {
        private readonly IApplicationDbContext context;
        private readonly ILogger logger;
        private readonly IFileStorageService fileStorageService;

        public CreateMedicalRecordCommandHandler(IApplicationDbContext context , ILogger logger , IFileStorageService fileStorageService)
        {
            this.context = context;
            this.logger = logger;
            this.fileStorageService = fileStorageService;
        }

        public async Task<Guid> Handle(CreateMedicalRecordCommand request , CancellationToken cancellationToken)
        {
            var patient = await context.PatientProfiles.FirstOrDefaultAsync(x => x.Id == request.Id,cancellationToken);

            if(patient == null) {
                throw new NotFoundException("Patient not found");
            }

            var hospital = await context.Hospitals.FirstOrDefaultAsync(x => x.Id == request.HospitalId,cancellationToken);

            if(hospital == null)
            {
                throw new NotFoundException("Hospitals not found");
            }

            var uploadResult = await fileStorageService.UploadAsync(request.File,"medicalrecords",cancellationToken);

            var medicalrecord = new MedicalRecord
            {
                Id = request.Id,
                HospitalId = request.HospitalId,
                FileName = uploadResult.FileName,
                FileUrl = uploadResult.FileURL,
                RecordType = request.RecordType
            };
            await context.MedicalRecords.AddAsync(medicalrecord);
            await context.SaveChangesAsync(cancellationToken);

            logger.LogInformation($"Medical Record Created - {request.Id}");

            return request.Id;
        }
    }
}
