

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SmartHealthcare.Application.Common.Settings;
using SmartHealthcare.Application.Contracts.Identity;
using SmartHealthcare.Application.Contracts.Services;
using SmartHealthcare.Infrastructure.Authentication;
using SmartHealthcare.Infrastructure.Email;
using SmartHealthcare.Infrastructure.Services;

namespace SmartHealthcare.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastucture(this IServiceCollection services,IConfiguration configuration)
        {
            services.AddScoped<IJwtTokenService, JwtTokenService>();

            services.AddScoped<IHospitalImportService,HospitalImportService>();

            services.AddScoped<ICurrentUserService,CurrentUserService>();

            services.AddHttpContextAccessor();

            services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));
            services.AddScoped<IEmailService, EmailService>();

            return services;
        }
    }
}
