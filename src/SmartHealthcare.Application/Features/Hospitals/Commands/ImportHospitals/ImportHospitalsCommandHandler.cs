

using MediatR;
using Microsoft.Extensions.Logging;
using SmartHealthcare.Application.Contracts.Services;

namespace SmartHealthcare.Application.Features.Hospitals.Commands.ImportHospitals
{
    public class ImportHospitalsCommandHandler:IRequestHandler<ImportHospitalsCommand>
    {
        private readonly IHospitalImportService hospitalImportService;
        private readonly ILogger logger;

        public ImportHospitalsCommandHandler(IHospitalImportService hospitalImportService , ILogger logger)
        {
            this.hospitalImportService = hospitalImportService;
            this.logger = logger;
        }

        public async Task<Unit> Handle(ImportHospitalsCommand request , CancellationToken cancellationToken)
        {
            await hospitalImportService.ImportHospitalsAsync(request.Filepath);

            logger.LogInformation($"Imported Hospital");

            return Unit.Value;
        }
    }

}
