using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WalletAPI.Services.Models;

namespace WalletAPI.Services.Data
{
    public static class PreSeeder
    {
        public static async Task Seeder(AppDbContext ctx, RoleManager<IdentityRole> roleManager, UserManager<User> userManager)
        {

            ctx.Database.EnsureCreated();
            if (!roleManager.Roles.Any())
            {
                var listOfRoles = new List<IdentityRole>
                {
                    new IdentityRole("admin"),
                    new IdentityRole("elite"),
                    new IdentityRole("noob")
                };
                foreach (var role in listOfRoles)
                {
                    await roleManager.CreateAsync(role);
                }
            }

            User admin;
            if (!userManager.Users.Any())
            {
                admin = new User
                {
                    Id = Guid.NewGuid().ToString(),
                    UserName = "randomuser@sample.com",
                    Email = "randomuser@sample.com",
                    LastName = "Scott",
                    FirstName = "Oscar",
                    PhoneNumber = "+2348061308168"
                };

                var result = await userManager.CreateAsync(admin, "01234Admin");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, "admin");
                }
            }
        }

    }
}
