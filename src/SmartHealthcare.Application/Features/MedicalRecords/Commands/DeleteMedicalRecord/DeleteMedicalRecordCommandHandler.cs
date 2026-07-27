using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartHealthcare.Application.Common.Exceptions;
using SmartHealthcare.Application.Common.Interface;
using SmartHealthcare.Application.Contracts.Persistence;

namespace SmartHealthcare.Application.Features.MedicalRecords.Commands.DeleteMedicalRecord
{
    public class DeleteMedicalRecordCommandHandler
        : IRequestHandler<DeleteMedicalRecordCommand, Unit>
    {
        private readonly IApplicationDbContext context;
        private readonly IFileStorageService fileStorageService;
        private readonly ILogger<DeleteMedicalRecordCommandHandler> logger;

        public DeleteMedicalRecordCommandHandler(
            IApplicationDbContext context,
            IFileStorageService fileStorageService,
            ILogger<DeleteMedicalRecordCommandHandler> logger)
        {
            this.context = context;
            this.fileStorageService = fileStorageService;
            this.logger = logger;
        }

        public async Task<Unit> Handle(
            DeleteMedicalRecordCommand request,
            CancellationToken cancellationToken)
        {
            var medicalRecord = await context.MedicalRecords
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (medicalRecord == null)
            {
                throw new NotFoundException("Medical record not found.");
            }

            // Delete the physical file first
            if (!string.IsNullOrWhiteSpace(medicalRecord.FileUrl))
            {
                await fileStorageService.DeleteAsync(medicalRecord.FileUrl);
            }

            // Delete the database record
            context.MedicalRecords.Remove(medicalRecord);

            await context.SaveChangesAsync(cancellationToken);

            logger.LogInformation(
                "Medical record {MedicalRecordId} deleted successfully.",
                request.Id);

            return Unit.Value;
        }
    }
}