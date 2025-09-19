using EcoShopApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcoShopApi.Infrastructure.Data.Config
{
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {

            builder.Property(p => p.Name)
                   .IsRequired()
                   .HasMaxLength(100);
            builder.Property(p => p.ProductCode)
                   .IsRequired()
                   .HasMaxLength(50);

            builder.HasIndex(p => p.ProductCode).IsUnique();

            builder.Property(p => p.Price)
                   .HasPrecision(18, 2);


            // Indexes
            builder.HasIndex(p => p.Name);




        }
    }
}