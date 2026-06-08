

using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using SmartHealthcare.Application.Contracts.Services;
using SmartHealthcare.Domain.Entities;
using System.Text;

namespace SmartHealthcare.Application.Features.Auth.Commands.ForgotPassword
{
    public class ForgotPasswordCommandHandler:IRequestHandler<ForgotPasswordCommand,bool>
    {
        private readonly IEmailService emailService;
        private readonly UserManager<ApplicationUser> userManager;

        public ForgotPasswordCommandHandler(UserManager<ApplicationUser> userManager , IEmailService emailService)
        {
            this.emailService = emailService;
            this.userManager = userManager;
        }

        public async Task<bool> Handle(ForgotPasswordCommand request , CancellationToken cancellationToken)
        {

            var user = await userManager.FindByEmailAsync(request.Email);

            if(user == null)
            {
                throw new UnauthorizedAccessException("User Not Found");
            }

            var token = await userManager.GeneratePasswordResetTokenAsync(user); 
            
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

            var resetLink = $"https://localhost:7183/api/Auth/reset-password?email={user.Email}&token={Uri.EscapeDataString(token)}";

            var body = $"""
                        <h3>Password Reset Request</h3>
                        <p>Click the link below to reset your password.</p>
                        <a href="{resetLink}">Reset Password</a>
                        <p>If you didn't request this, ignore this email.</p>
                        """;

            await emailService.SendEmailAsync(user.Email! ,"Password Reset", resetLink);

            return true;
                
        }
    }
}
