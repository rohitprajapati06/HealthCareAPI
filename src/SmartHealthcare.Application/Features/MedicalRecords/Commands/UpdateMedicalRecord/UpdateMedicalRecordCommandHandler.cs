using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartHealthcare.Application.Common.Exceptions;
using SmartHealthcare.Application.Common.Interface;
using SmartHealthcare.Application.Common.Models;
using SmartHealthcare.Application.Contracts.Persistence;

namespace SmartHealthcare.Application.Features.MedicalRecords.Commands.UpdateMedicalRecord
{
    public class UpdateMedicalRecordCommandHandler
        : IRequestHandler<UpdateMedicalRecordCommand, Unit>
    {
        private readonly IApplicationDbContext context;
        private readonly IFileStorageService fileStorageService;
        private readonly ILogger<UpdateMedicalRecordCommandHandler> logger;

        public UpdateMedicalRecordCommandHandler(
            IApplicationDbContext context,
            IFileStorageService fileStorageService,
            ILogger<UpdateMedicalRecordCommandHandler> logger)
        {
            this.context = context;
            this.fileStorageService = fileStorageService;
            this.logger = logger;
        }

        public async Task<Unit> Handle(
            UpdateMedicalRecordCommand request,
            CancellationToken cancellationToken)
        {
            var medicalRecord = await context.MedicalRecords
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (medicalRecord == null)
            {
                throw new NotFoundException("Medical record not found.");
            }

            string? oldFileUrl = null;
            FileUploadResult? newUpload = null;

            try
            {
                if (request.File != null)
                {
                    oldFileUrl = medicalRecord.FileUrl;

                    newUpload = await fileStorageService.UploadAsync(
                        request.File,
                        "medicalrecords",
                        cancellationToken);

                    medicalRecord.FileName = newUpload.FileName;
                    medicalRecord.FileUrl = newUpload.FileURL;
                }

                medicalRecord.RecordType = request.RecordType;

                await context.SaveChangesAsync(cancellationToken);

                if (!string.IsNullOrWhiteSpace(oldFileUrl))
                {
                    await fileStorageService.DeleteAsync(oldFileUrl);
                }

                logger.LogInformation(
                    "Medical record {MedicalRecordId} updated successfully.",
                    medicalRecord.Id);

                return Unit.Value;
            }
            catch
            {
                if (newUpload != null)
                {
                    await fileStorageService.DeleteAsync(newUpload.FileURL);
                }

                throw;
            }
        }
    }
}