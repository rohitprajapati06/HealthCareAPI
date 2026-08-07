

using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartHealthcare.Application.Common.Exceptions;
using SmartHealthcare.Application.Contracts.Persistence;
using SmartHealthcare.Application.Features.Doctors.Responses;
using SmartHealthcare.Domain.Enums;

namespace SmartHealthcare.Application.Features.Doctors.Queries.GetDoctorsById
{
    public class GetDoctorByIdQueryHandler:IRequestHandler<GetDoctorsByIdQuery,DoctorResponse>
    {
        private readonly IApplicationDbContext dbContext;

        public GetDoctorByIdQueryHandler(IApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<DoctorResponse> Handle(GetDoctorsByIdQuery request , CancellationToken cancellationToken)
        {
            var doctor = await dbContext.DoctorProfiles
                .AsNoTracking()
                .Where(x => x.Id == request.Id)
                .Select(x => new DoctorResponse
                {
                    Id = x.Id,
                    UserId = x.UserId,
                    Email = x.User.Email,
                    FirstName = x.User.FirstName,
                    LastName = x.User.LastName,
                    Specialization = x.Specialization,
                    ExperienceYears = x.ExperienceYears,
                    HospitalName = x.Hospital.Name,
                    ConsultationFee = x.ConsultationFee,
                    Qualification = x.Qualification
                }).FirstOrDefaultAsync(cancellationToken);

            if(doctor == null)
            {
                throw new NotFoundException("Doctor not found");
            }

            return doctor;
            
        }
    }
}
