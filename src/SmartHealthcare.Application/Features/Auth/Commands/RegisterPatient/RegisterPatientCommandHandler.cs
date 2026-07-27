using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using SmartHealthcare.Application.Common.Exceptions;
using SmartHealthcare.Application.Contracts.Persistence;
using SmartHealthcare.Application.Features.Auth.Commands.Login;
using SmartHealthcare.Domain.Entities;

namespace SmartHealthcare.Application.Features.Auth.Commands.RegisterPatient
{
    public class RegisterPatientCommandHandler:IRequestHandler<RegisterPatientCommand,Guid>
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IApplicationDbContext context;
        private readonly ILogger<RegisterPatientCommandHandler> logger1;
        private readonly ILogger<LoginCommandHandler> logger;

        public RegisterPatientCommandHandler(UserManager<ApplicationUser> userManager ,IApplicationDbContext context, ILogger<RegisterPatientCommandHandler> logger  )
        {
            this.userManager = userManager;
            this.context = context;
            logger1 = logger;
        }

        public async Task<Guid> Handle(RegisterPatientCommand request, CancellationToken cancellationToken)
        {

            var existingUser = await userManager.FindByEmailAsync(request.Email);

            if(existingUser != null)
            {
                logger.LogWarning($"Failed login attempt for Email {request.Email} ");
                throw new ConflictException("Email Already Exists");
            }

            var user = new ApplicationUser
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                UserName = request.Email,
                PhoneNumber = request.PhoneNumber,
                IsActive = true
            };

            var result = await userManager.CreateAsync(user,request.Password);

            if (!result.Succeeded) 
            {
                throw new BadRequestException( string.Join(" ",result.Errors.Select(x => x.Description)));
            }

            await userManager.AddToRoleAsync(user,"Patient");

            var patientprofile = new PatientProfile
            {
                UserId = user.Id,
                DateOfBirth = request.DateofBirth,
                Gender = request.Gender,
                BloodGroup = request.BloodGroup,
                Allergies = request.Allergies,
                ExistingConditions = request.ExistingConditions
            };

            await context.PatientProfiles.AddAsync(patientprofile,cancellationToken);
            await context.SaveChangesAsync(cancellationToken);

            logger.LogInformation($"{user.Id} patient has been successfully registered");

            return user.Id;
        }
    }
}
