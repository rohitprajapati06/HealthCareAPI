

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using SmartHealthcare.Domain.Enums;

namespace SmartHealthcare.Persistence.Seed
{
    public static class RoleSeeder
    {

        public static async Task SeedRolesAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

            string[] roles =
            {
                UserRoles.Doctor,
                UserRoles.Patient,
                UserRoles.SuperAdmin,
                UserRoles.HospitalAdmin
            };

            foreach (var role in roles)
            {

                var roleExists = await roleManager.RoleExistsAsync(role);


                if (!roleExists)
                {
                    await roleManager.CreateAsync(new IdentityRole<Guid>(role));
                }
            }
        }
    }
}
