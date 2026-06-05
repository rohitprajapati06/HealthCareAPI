

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SmartHealthcare.Application.Contracts.Identity;
using SmartHealthcare.Application.Contracts.Services;
using SmartHealthcare.Infrastructure.Authentication;
using SmartHealthcare.Infrastructure.Services;

namespace SmartHealthcare.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastucture(this IServiceCollection services,IConfiguration configuration)
        {
            services.AddScoped<IJwtTokenService, JwtTokenService>();

            services.AddScoped<IHospitalImportService,HospitalImportService>();

            return services;
        }
    }
}
