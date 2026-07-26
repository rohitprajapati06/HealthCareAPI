

using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using SmartHealthcare.Application.Common.Exceptions;
using SmartHealthcare.Application.Contracts.Persistence;
using SmartHealthcare.Application.Features.Auth.Commands.RegisterPatient;
using SmartHealthcare.Domain.Entities;

namespace SmartHealthcare.Application.Features.Auth.Commands.RegisterDoctor
{
    public class RegisterDoctorCommandHandler:IRequestHandler<RegisterDoctorCommand,Guid>
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IApplicationDbContext context;
        private readonly ILogger<RegisterDoctorCommandHandler> logger;

        public RegisterDoctorCommandHandler(UserManager<ApplicationUser> userManager , IApplicationDbContext context , ILogger<RegisterDoctorCommandHandler> logger) 
        {
            this.userManager = userManager;
            this.context = context;
            this.logger = logger;
        }

        public async Task<Guid> Handle(RegisterDoctorCommand request , CancellationToken cancellationToken)
        {
            var existingUser = await userManager.FindByEmailAsync(request.Email);

            if (existingUser != null)
            {
                throw new ConflictException("Email already exists");
            }

            var user = new ApplicationUser
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                UserName = request.Email,
                PhoneNumber = request.PhoneNumber,
                IsActive = true,
                HospitalId = request.HospitalId,
            };

            var result = await userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded) 
            {
                throw new BadRequestException(string.Join(" ",result.Errors.Select(x => x.Description)));
            }

            await userManager.AddToRoleAsync(user, "Doctor");

            var doctorprofiles = new DoctorProfile
            {
                UserId = user.Id,
                HospitalId = request.HospitalId,
                ExperienceYears = request.ExperienceYears,
                ConsultationFee = request.ConsultationFee,
                Specialization = request.Specialization,
                Qualification = request.Qualification,

            };

            await context.DoctorProfiles.AddAsync(doctorprofiles,cancellationToken);
            await context.SaveChangesAsync(cancellationToken);

            logger.LogInformation($"{user.Id} {user.HospitalId} doctor has been successfully registered");

            return user.Id;
        }
    }
}
