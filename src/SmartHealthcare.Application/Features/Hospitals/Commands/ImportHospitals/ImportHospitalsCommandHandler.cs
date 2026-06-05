

using MediatR;
using SmartHealthcare.Application.Contracts.Services;

namespace SmartHealthcare.Application.Features.Hospitals.Commands.ImportHospitals
{
    public class ImportHospitalsCommandHandler:IRequestHandler<ImportHospitalsCommand>
    {
        private readonly IHospitalImportService hospitalImportService;

        public ImportHospitalsCommandHandler(IHospitalImportService hospitalImportService)
        {
            this.hospitalImportService = hospitalImportService;
        }

        public async Task<Unit> Handle(ImportHospitalsCommand request , CancellationToken cancellationToken)
        {
            await hospitalImportService.ImportHospitalsAsync(request.Filepath);

            return Unit.Value;
        }
    }

}
