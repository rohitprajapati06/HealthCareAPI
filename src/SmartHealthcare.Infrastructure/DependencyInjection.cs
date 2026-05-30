

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SmartHealthcare.Application.Contracts.Identity;
using SmartHealthcare.Infrastructure.Authentication;

namespace SmartHealthcare.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastucture(this IServiceCollection services,IConfiguration configuration)
        {
            services.AddScoped<IJwtTokenService, JwtTokenService>();

            return services;
        }
    }
}
