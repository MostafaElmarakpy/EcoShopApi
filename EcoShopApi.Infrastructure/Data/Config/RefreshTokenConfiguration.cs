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
       public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
       {
              public void Configure(EntityTypeBuilder<RefreshToken> builder)
              {
                     builder.HasKey(rt => rt.Id);

                     builder.Property(rt => rt.Token)
                            .IsRequired()
                            .HasMaxLength(200);

                     builder.Property(rt => rt.ExpireAt)
                            .IsRequired();

                     builder.Property(rt => rt.IsRevoked)
                            .IsRequired();

                     builder.HasOne(rt => rt.User)
                            .WithMany(u => u.RefreshTokens)
                            .HasForeignKey(rt => rt.UserId)
                            .OnDelete(DeleteBehavior.Cascade);

                     builder.HasIndex(rt => rt.Token).IsUnique();

              }
       }
}