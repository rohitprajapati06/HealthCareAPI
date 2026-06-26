

using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartHealthcare.Application.Contracts.Persistence;
using SmartHealthcare.Application.Features.Doctors.Responses;
using SmartHealthcare.Domain.Enums;

namespace SmartHealthcare.Application.Features.Doctors.Queries.GetDoctorsById
{
    public class GetDoctorByIdQueryHandler:IRequestHandler<GetDoctorsByIdQuery,List<DoctorResponse>>
    {
        private readonly IApplicationDbContext dbContext;

        public GetDoctorByIdQueryHandler(IApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<List<DoctorResponse>> Handle(GetDoctorsByIdQuery request , CancellationToken cancellationToken)
        {
            return await dbContext.DoctorProfiles
                .Include(x => x.User)
                .Include(x => x.Hospital)
                .Where(x => x.ApprovalStatus == DoctorApprovalStatus.Approved)
                .Select(x => new DoctorResponse
                {
                    Id = x.User.Id,
                    UserId = x.User.Id,
                    Email = x.User.Email,
                    FirstName = x.User.FirstName,
                    LastName = x.User.LastName,
                    ExperienceYears = x.ExperienceYears,
                    Qualification = x.Qualification,
                    HospitalName = x.Hospital.Name,
                    Specialization = x.Specialization,           
                    ConsultationFee = x.ConsultationFee,
                    
                    
                }).ToListAsync(cancellationToken);
        }
    }
}
