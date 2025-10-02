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

                // User and role setup (existing code)
                if (!_roleManager.RoleExistsAsync(SD.Role_Admin).GetAwaiter().GetResult())
                {
                    _roleManager.CreateAsync(new IdentityRole(SD.Role_Admin)).Wait();
                    _roleManager.CreateAsync(new IdentityRole(SD.Role_User)).Wait();

                    _userManager.CreateAsync(new AppUser
                    {
                        UserName = "admin@dotnetmastery.com",
                        Email = "admin@dotnetmastery.com",
                        DisplayName = "Mostafa Ramadan",
                        NormalizedUserName = "ADMIN@DOTNETMASTERY.COM",
                        NormalizedEmail = "ADMIN@DOTNETMASTERY.COM",
                    }, "Admin123*").GetAwaiter().GetResult();

                    AppUser user = _db.Users.FirstOrDefault(u => u.Email == "admin@dotnetmastery.com");
                    _userManager.AddToRoleAsync(user, SD.Role_Admin).GetAwaiter().GetResult();
                }

                // Seed categories idempotently
                if (!_db.Categories.Any(c => c.Name == "Laptops"))
                {
                    _db.Categories.Add(new Category
                    {
                        Name = "Laptops",
                        Description = "Portable computers for productivity and entertainment"
                    });
                }

                if (!_db.Categories.Any(c => c.Name == "Smartphones"))
                {
                    _db.Categories.Add(new Category
                    {
                        Name = "Smartphones",
                        Description = "Modern mobile devices with advanced features"
                    });
                }

                if (!_db.Categories.Any(c => c.Name == "Accessories"))
                {
                    _db.Categories.Add(new Category
                    {
                        Name = "Accessories",
                        Description = "Gadgets and add-ons for electronic devices"
                    });
                }

                _db.SaveChanges();

                // Fetch categories WITHOUT tracking to avoid conflicts
                var laptopsCategory = _db.Categories.AsNoTracking().FirstOrDefault(c => c.Name == "Laptops");
                var smartphonesCategory = _db.Categories.AsNoTracking().FirstOrDefault(c => c.Name == "Smartphones");
                var accessoriesCategory = _db.Categories.AsNoTracking().FirstOrDefault(c => c.Name == "Accessories");

                // Seed products idempotently
                if (laptopsCategory != null && !_db.Products.Any(p => p.ProductCode == "LAP001"))
                {
                    _db.Products.Add(new Product
                    {
                        ProductCode = "LAP001",
                        Name = "MacBook Pro",
                        ImagePath = null,
                        Price = 2000.00M,
                        MinimumQuantity = 1,
                        DiscountRate = 0.05,
                        CategoryId = laptopsCategory.Id
                    });
                }

                if (laptopsCategory != null && !_db.Products.Any(p => p.ProductCode == "LAP002"))
                {
                    _db.Products.Add(new Product
                    {
                        ProductCode = "LAP002",
                        Name = "Dell XPS 13",
                        ImagePath = null,
                        Price = 1500.00M,
                        MinimumQuantity = 1,
                        DiscountRate = 0.10,
                        CategoryId = laptopsCategory.Id
                    });
                }

                if (smartphonesCategory != null && !_db.Products.Any(p => p.ProductCode == "SPH001"))
                {
                    _db.Products.Add(new Product
                    {
                        ProductCode = "SPH001",
                        Name = "iPhone 14",
                        ImagePath = null,
                        Price = 999.00M,
                        MinimumQuantity = 1,
                        DiscountRate = 0.05,
                        CategoryId = smartphonesCategory.Id
                    });
                }

                if (smartphonesCategory != null && !_db.Products.Any(p => p.ProductCode == "SPH002"))
                {
                    _db.Products.Add(new Product
                    {
                        ProductCode = "SPH002",
                        Name = "Samsung Galaxy S23",
                        ImagePath = null,
                        Price = 899.00M,
                        MinimumQuantity = 1,
                        DiscountRate = 0.08,
                        CategoryId = smartphonesCategory.Id
                    });
                }

                if (accessoriesCategory != null && !_db.Products.Any(p => p.ProductCode == "ACC001"))
                {
                    _db.Products.Add(new Product
                    {
                        ProductCode = "ACC001",
                        Name = "Wireless Charger",
                        ImagePath = null,
                        Price = 49.99M,
                        MinimumQuantity = 1,
                        DiscountRate = 0.15,
                        CategoryId = accessoriesCategory.Id
                    });
                }

                _db.SaveChanges();
            }
            catch (Exception e)
            {
                throw;
            }
        }

    }
}