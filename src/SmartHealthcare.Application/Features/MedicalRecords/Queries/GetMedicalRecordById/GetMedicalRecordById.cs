

using MediatR;
using SmartHealthcare.Application.Features.MedicalRecords.Responses;

namespace SmartHealthcare.Application.Features.MedicalRecords.Queries.GetMedicalRecordById
{
    public record GetMedicalRecordById(Guid Id) : IRequest<MedicalRecordResponse>;
    
}
