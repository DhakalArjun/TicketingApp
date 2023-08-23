using Microsoft.AspNetCore.Identity;
using TicketingApp.Constants;
using TicketingApp.Data;
using TicketingApp.Models;

namespace TicketingApp.Data
{
    public static class DbSeeder
    {
        
        public static async Task SeedRolesAndAdminAsync(IServiceProvider service)
        {
            //Seed Roles
            var userManager = service.GetService<UserManager<ApplicationUser>>();
            var roleManager = service.GetService<RoleManager<IdentityRole>>();

            await roleManager.CreateAsync(new IdentityRole(Roles.Role_Admin.ToString()));
            await roleManager.CreateAsync(new IdentityRole(Roles.Role_User.ToString()));
            await roleManager.CreateAsync(new IdentityRole(Roles.Role_Manager.ToString()));
            await roleManager.CreateAsync(new IdentityRole(Roles.Role_Agent.ToString()));

            // creating admin

            var admin = new ApplicationUser
            {
                UserName = "admin@gmail.com",
                Email = "admin@gmail.com",
                FirstName = "Arjun",
                LastName = "Dhakal",
                EmailConfirmed = true,
                PhoneNumberConfirmed = true
            };
            var userInDb = await userManager.FindByEmailAsync(admin.Email);
            if (userInDb == null)
            {
                await userManager.CreateAsync(admin, "Admin@123");
                await userManager.AddToRoleAsync(admin, Roles.Role_Admin.ToString());                
            }
        } 
    }
}
