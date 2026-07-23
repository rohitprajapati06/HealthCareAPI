
using MediatR;

namespace SmartHealthcare.Application.Features.MedicalRecords.Commands.DeleteMedicalRecord
{
    public record DeleteMedicalRecordCommand(Guid Id) : IRequest<Unit>;
    
}
