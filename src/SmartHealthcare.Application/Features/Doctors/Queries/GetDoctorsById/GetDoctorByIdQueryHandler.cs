

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
            var doctors = await dbContext.DoctorProfiles
                .Include(x => x.User)
                .Include(x => x.Hospital)
                .FirstOrDefaultAsync(x => x.Id == request.Id,cancellationToken);

            if(doctors == null)
            {
                throw new NotFoundException("Doctor Not Found");
            }

            return new DoctorResponse
            {
                Id = doctors.Id,
                UserId = doctors.UserId,
                Email = doctors.User.Email,
                FirstName = doctors.User.FirstName,
                LastName = doctors.User.LastName,
                Specialization = doctors.Specialization,
                ExperienceYears = doctors.ExperienceYears,
                HospitalName = doctors.Hospital.Name,   
                ConsultationFee = doctors.ConsultationFee,
                Qualification = doctors.Qualification,
            };
            
        }
    }
}
