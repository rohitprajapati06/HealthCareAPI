

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using SmartHealthcare.Domain.Entities;
using SmartHealthcare.Domain.Enums;

namespace SmartHealthcare.Persistence.Seed
{
    public static class SuperAdminSeeder
    {
        public static async Task SuperAdminSeederAsync(IServiceProvider serviceProvider) {

            var manager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            const string email = "superadmin@smarthealthcare.com";

            var existingUser = await manager.FindByEmailAsync(email);

            if (existingUser != null) return;


            var superadmin = new ApplicationUser
            {
                FirstName = "System",
                LastName = "Administrator",
                Email = email,
                UserName = email,
                IsActive = true,
                EmailConfirmed = true,
            };

            var result = await manager.CreateAsync(superadmin,"Admin@123");

            if (result.Succeeded) {

                await manager.AddToRoleAsync(superadmin, UserRoles.SuperAdmin);
            }



        }
    }
}
