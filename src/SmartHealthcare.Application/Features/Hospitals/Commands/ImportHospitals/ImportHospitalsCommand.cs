

using MediatR;
using Microsoft.AspNetCore.Http;

namespace SmartHealthcare.Application.Features.Hospitals.Commands.ImportHospitals
{
    public class ImportHospitalsCommand:IRequest
    {
        public IFormFile File { get; set; } = null!;
    }
}
