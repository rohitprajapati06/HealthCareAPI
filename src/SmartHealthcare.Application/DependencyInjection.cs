using System.Reflection;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace SmartHealthcare.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services) 
        {
            services.AddMediatR(typeof(DependencyInjection).Assembly);
            services.AddValidatorsFromAssemblies(
                new[] { Assembly.GetExecutingAssembly() });
            
            return services;

        }
    }
}
