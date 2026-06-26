

using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartHealthcare.Application.Contracts.Persistence;
using SmartHealthcare.Application.Features.Doctors.Responses;
using SmartHealthcare.Domain.Enums;

namespace SmartHealthcare.Application.Features.Doctors.Queries.SearchDoctors
{
    public class SearchDoctorsQueryHandler : IRequestHandler<SearchDoctorsQuery,List<DoctorResponse>>
    {
        private readonly IApplicationDbContext context;

        public SearchDoctorsQueryHandler(IApplicationDbContext context)
        {
            this.context = context;
        }

        public async Task<List<DoctorResponse>> Handle(SearchDoctorsQuery request , CancellationToken cancellationToken)
        {
            var doctors =  context.DoctorProfiles
               .Include(x => x.User)
               .Include(x => x.Hospital)
               .Where(x => x.ApprovalStatus == DoctorApprovalStatus.Approved)
               .AsQueryable();

            if (request.HospitalId.HasValue)
            {
                doctors = doctors.Where(x => x.HospitalId == request.HospitalId);
            }

            if (!string.IsNullOrWhiteSpace(request.Specialization))
            {
                doctors = doctors.Where(x => x.Specialization == request.Specialization);
            }

            if (request.Experience.HasValue)
            {
                doctors = doctors.Where(x => x.ExperienceYears == request.Experience);
            }

            if (request.MinFee.HasValue)
            {
                doctors = doctors.Where(x => x.ConsultationFee >= request.MinFee);
            }

            if (request.MaxFee.HasValue)
            {
                doctors = doctors.Where(x => x.ConsultationFee <= request.MaxFee); 
            }

            return await doctors
                .Select(x => new DoctorResponse
                {
                    Id = x.Id,
                    UserId  = x.User.Id,
                    FirstName = x.User.FirstName,
                    LastName = x.User.LastName,
                    Email = x.User.Email,
                    HospitalName = x.Hospital.Name,
                    ExperienceYears = x.ExperienceYears,
                    Qualification = x.Qualification,
                    Specialization = x.Specialization,
                    ConsultationFee = x.ConsultationFee
                }).ToListAsync(cancellationToken);
        }
    }
}
