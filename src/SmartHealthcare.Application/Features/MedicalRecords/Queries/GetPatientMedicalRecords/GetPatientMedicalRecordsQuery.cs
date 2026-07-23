using MediatR;
using SmartHealthcare.Application.Features.MedicalRecords.Responses;


namespace SmartHealthcare.Application.Features.MedicalRecords.Queries.GetPatientMedicalRecords
{
    public record GetPatientMedicalRecordsQuery(Guid PatientId) : IRequest<List<MedicalRecordResponse>>;
    
}
