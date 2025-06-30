using BulkyBook.DataAccess.Data;
using BulkyBook.Models;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.DataAccess.DBInitializer
{
    public class DbInitializer : IDbInitializer
    {
        private readonly UserManager<IdentityUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly ApplicationDbContext context;
        public DbInitializer(UserManager<IdentityUser> userManager,
             RoleManager<IdentityRole> roleManager,
             ApplicationDbContext context
            )
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.context = context;
        }
        public async Task Initialize()
        {
            try
            {
                var Migration = await context.Database.GetPendingMigrationsAsync();
                if (Migration.Count() > 0)
                {
                   await context.Database.MigrateAsync();
                }

            }
            catch(Exception) { }

            if (!await roleManager.RoleExistsAsync(SD.Role_Customer))
            {
                await roleManager.CreateAsync(new IdentityRole(SD.Role_Customer));
                await roleManager.CreateAsync(new IdentityRole(SD.Role_Employee));
                await roleManager.CreateAsync(new IdentityRole(SD.Role_Admin));
                await roleManager.CreateAsync(new IdentityRole(SD.Role_Company));

                await userManager.CreateAsync(new ApplicationUser
                {
                    UserName = "admin@dotnetmastery.com",
                    Email = "admin@dotnetmastery.com",
                    Name = "Bhrugen Patel",
                    PhoneNumber = "1112223333",
                    StreetAddress = "test 123 Ave",
                    State = "IL",
                    PostalCode = "23422",
                    City = "Chicago"
                }, "Admin123*");
                ApplicationUser user = await context.ApplicationUsers.FirstOrDefaultAsync(u => u.Email == "admin@dotnetmastery.com");
                await userManager.AddToRoleAsync(user, SD.Role_Admin);
                return;
            }
        }
    }
}