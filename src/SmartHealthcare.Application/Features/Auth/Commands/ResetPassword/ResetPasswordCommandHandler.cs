using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using SmartHealthcare.Domain.Entities;
using System.Text;


namespace SmartHealthcare.Application.Features.Auth.Commands.ResetPassword
{
    public class ResetPasswordCommandHandler:IRequestHandler<ResetPasswordCommand,bool>
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly ILogger<ResetPasswordCommandHandler> logger;

        public ResetPasswordCommandHandler(UserManager<ApplicationUser> userManager , ILogger<ResetPasswordCommandHandler> logger)
        {
            this.userManager = userManager;
            this.logger = logger;
        }

        public async Task<bool> Handle(ResetPasswordCommand request,CancellationToken cancellationToken)
        {
            var user = await userManager.FindByEmailAsync(request.Email);
            if(user == null)
            {
                throw new UnauthorizedAccessException("User not found");
            }
            var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(request.Token));

            var result = await userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
            if (!result.Succeeded)
            {
                throw new Exception(String.Join(" ", result.Errors.Select(x => x.Description)));
            }

            logger.LogInformation($" Password reset request is invoked for {user.Id} ");

            return true;
        }
    }
}
