using MediatR;
using Microsoft.Extensions.Logging;
using SmartHealthcare.Application.Contracts.Services;

namespace SmartHealthcare.Application.Features.Hospitals.Commands.ImportHospitals
{
    public class ImportHospitalsCommandHandler : IRequestHandler<ImportHospitalsCommand>
    {
        private readonly IHospitalImportService hospitalImportService;
        private readonly ILogger<ImportHospitalsCommandHandler> logger;

        public ImportHospitalsCommandHandler(
            IHospitalImportService hospitalImportService,
            ILogger<ImportHospitalsCommandHandler> logger)
        {
            this.hospitalImportService = hospitalImportService;
            this.logger = logger;
        }

        public async Task<Unit> Handle(
            ImportHospitalsCommand request,
            CancellationToken cancellationToken)
        {
            await using var stream = request.File.OpenReadStream();

            var importedCount = await hospitalImportService
                .ImportHospitalsAsync(stream, cancellationToken);

            logger.LogInformation(
                "Successfully imported {ImportedCount} hospitals.",
                importedCount);

            return Unit.Value;
        }
    }
}