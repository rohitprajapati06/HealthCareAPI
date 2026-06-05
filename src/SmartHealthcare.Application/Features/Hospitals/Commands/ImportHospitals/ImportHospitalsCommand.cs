

using MediatR;

namespace SmartHealthcare.Application.Features.Hospitals.Commands.ImportHospitals
{
    public class ImportHospitalsCommand:IRequest
    {
        public string Filepath { get; set; } = string.Empty;
    }
}
