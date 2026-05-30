using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SmartHealthcare.Application.Contracts.Persistence;
using SmartHealthcare.Persistence.Contexts;

namespace SmartHealthcare.Persistence
{
    public static class DependencyInjection
    {

        public static IServiceCollection AddPersistance(this IServiceCollection services ,IConfiguration configuration)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            services.AddScoped<IApplicationDbContext,ApplicationDbContext>();

            return services;
        }
    }
}
