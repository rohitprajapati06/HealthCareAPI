
using MediatR;
using Microsoft.AspNetCore.Http;

namespace SmartHealthcare.Application.Features.MedicalRecords.Commands.UpdateMedicalRecord
{
    public class UpdateMedicalRecordCommand : IRequest<Unit>
    {
        public Guid Id { get; set; }

        public IFormFile? File { get; set; }

        public string RecordType { get; set; } = string.Empty;
    }
}
