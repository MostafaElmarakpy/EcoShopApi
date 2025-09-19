using EcoShopApi.Application.Common.Interfaces;
using EcoShopApi.Application.Common.Utility;
using EcoShopApi.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace EcoShopApi.Infrastructure.Data
{
    public class DbInitializer : IDbInitializer
    {

        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _db;

        public DbInitializer(
            UserManager<AppUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext db)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _db = db;
        }

        public void Initialize()
        {
            try
            {
                if (_db.Database.GetPendingMigrations().Count() > 0)
                {
                    _db.Database.Migrate();
                }

                if (!_roleManager.RoleExistsAsync(SD.Role_Admin).GetAwaiter().GetResult())
                {
                    _roleManager.CreateAsync(new IdentityRole(SD.Role_Admin)).Wait();
                    _roleManager.CreateAsync(new IdentityRole(SD.Role_User)).Wait();
                    _userManager.CreateAsync(new AppUser
                    {
                        UserName = "admin@dotnetmastery.com",
                        Email = "admin@dotnetmastery.com",
                        DisplayName = "Bhrugen Patel",
                        NormalizedUserName = "ADMIN@DOTNETMASTERY.COM",
                        NormalizedEmail = "ADMIN@DOTNETMASTERY.COM",
                    }, "Admin123*").GetAwaiter().GetResult();

                    AppUser user = _db.Users.FirstOrDefault(u => u.Email == "admin@dotnetmastery.com");
                    _userManager.AddToRoleAsync(user, SD.Role_Admin).GetAwaiter().GetResult();
                }
            }
            catch (Exception e)
            {
                throw;
            }
        }
    }
}