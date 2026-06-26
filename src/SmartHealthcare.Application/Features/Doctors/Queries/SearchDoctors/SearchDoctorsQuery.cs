using MediatR;
using SmartHealthcare.Application.Features.Doctors.Responses;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartHealthcare.Application.Features.Doctors.Queries.SearchDoctors
{
    public record SearchDoctorsQuery(Guid? HospitalId, string? Specialization,
        int? Experience, decimal? MaxFee, decimal? MinFee) : IRequest<List<DoctorResponse>>;
 
}
