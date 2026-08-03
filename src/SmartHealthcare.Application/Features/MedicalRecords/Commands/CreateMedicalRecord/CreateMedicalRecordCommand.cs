using MediatR;
using Microsoft.AspNetCore.Http;

namespace SmartHealthcare.Application.Features.MedicalRecords.Commands.CreateMedicalRecord
{
    public class CreateMedicalRecordCommand : IRequest<Guid>
    {
        public Guid PatientId { get; set; }

        public Guid HospitalId { get; set; }

        public IFormFile File { get; set; } = default!;

        public string RecordType { get; set; } = string.Empty;
    }
}
