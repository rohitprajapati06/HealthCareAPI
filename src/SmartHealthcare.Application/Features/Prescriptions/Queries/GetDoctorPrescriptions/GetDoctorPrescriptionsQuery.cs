using MediatR;
using SmartHealthcare.Application.Features.Prescriptions.Responses;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartHealthcare.Application.Features.Prescriptions.Queries.GetDoctorPrescriptions
{
    public record GetDoctorPrescriptionsQuery(Guid DoctorId) : IRequest <List<PrescriptionsResponses>>;
    
}
