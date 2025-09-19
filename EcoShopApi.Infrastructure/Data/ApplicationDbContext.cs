using EcoShopApi.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EcoShopApi.Infrastructure.Data
{
    public class ApplicationDbContext(DbContextOptions options) : IdentityDbContext<AppUser>(options)
    {
        public DbSet<AppUser> appUsers => Set<AppUser>();
        public DbSet<Product> Products => Set<Product>();

        public DbSet<Category> Categories { get; set; }
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<AppUser>().HasIndex(u => u.UserName).IsUnique();
            modelBuilder.Entity<AppUser>().HasIndex(u => u.Email).IsUnique();


            modelBuilder.ApplyConfigurationsFromAssembly(
                Assembly.GetExecutingAssembly());
        }
    }
}
