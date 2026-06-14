using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using SmartHealthcare.Application.Contracts.Identity;
using SmartHealthcare.Domain.Entities;


namespace SmartHealthcare.Application.Features.Auth.Commands.ChangePassword
{
    public class ChangePasswordCommandHandler:IRequestHandler<ChangePasswordCommand,bool>
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly ICurrentUserService currentUser;
        private readonly ILogger<ChangePasswordCommandHandler> logger;

        public ChangePasswordCommandHandler(UserManager<ApplicationUser> userManager,ICurrentUserService currentUser ,ILogger<ChangePasswordCommandHandler> logger)
        {
            this.userManager = userManager;
            this.currentUser = currentUser;
            this.logger = logger;
        }

        public async Task<bool> Handle(ChangePasswordCommand request , CancellationToken cancellationToken)
        {
            var userId =  currentUser.UserId;

            if(userId == null)
            {
                throw new UnauthorizedAccessException("User Not Authorize");
            }

            var user = await userManager.FindByIdAsync(userId.ToString());

            if(user == null)
            {
                throw new UnauthorizedAccessException("User Not Found");
            }

            var result = await userManager.ChangePasswordAsync(user, request.CurrentPassword , request.NewPassword);

            if (!result.Succeeded)
            {
                throw new Exception(String.Join(" ", result.Errors.Select(x => x.Description)));
            }

            logger.LogInformation($" Password has been changed for {user.Id}");

            return true;
        }
    }
}
