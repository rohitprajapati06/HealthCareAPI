

using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartHealthcare.Application.Contracts.Persistence;
using SmartHealthcare.Application.Features.Doctors.Responses;
using SmartHealthcare.Domain.Enums;

namespace SmartHealthcare.Application.Features.Doctors.Queries.GetDoctors
{
    public class GetDoctorsQueryHandler : IRequestHandler<GetDoctorsQuery,List<DoctorResponse>>
    {
        private readonly IApplicationDbContext context;

        public GetDoctorsQueryHandler(IApplicationDbContext context)
        {
            this.context = context;
        }

        public async Task<List<DoctorResponse>> Handle(GetDoctorsQuery request , CancellationToken cancellationToken)
        {
            return await context.DoctorProfiles
                .Include(x => x.User)
                .Include(x => x.Hospital)
                .Where(x => x.ApprovalStatus == DoctorApprovalStatus.Approved)
                .Select(x => new DoctorResponse
                {

                    Id = x.Id,
                    UserId = x.User.Id,
                    FirstName = x.User.FirstName,
                    LastName = x.User.LastName,
                    Email = x.User.Email,
                    Specialization = x.Specialization,
                    Qualification = x.Qualification,
                    ConsultationFee = x.ConsultationFee,
                    ExperienceYears = x.ExperienceYears,
                    HospitalName = x.Hospital.Name

                }).ToListAsync(cancellationToken);
        }
    }
}
