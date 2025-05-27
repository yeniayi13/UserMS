using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Domain.Entities.Support;
using System.Diagnostics.CodeAnalysis;

namespace UserMs.Infrastructure.Database.Configuration.Postgres
{
    [ExcludeFromCodeCoverage]
    public class SupportsConfiguration : IEntityTypeConfiguration<Supports>
    {
        public void Configure(EntityTypeBuilder<Supports> builder)
        {
            builder.HasKey(s => s.UserId);
            builder.Property(s => s.UserEmail).IsRequired();
            builder.Property(s => s.UserPassword).IsRequired();
            builder.Property(s => s.UserName).IsRequired();
            builder.Property(s => s.UserPhone).IsRequired();
            builder.Property(s => s.UserAddress).IsRequired();
            builder.Property(s => s.UserLastName).IsRequired();
            builder.Property(s => s.SupportDni).IsRequired();
            builder.Property(s => s.SupportSpecialization).IsRequired().HasMaxLength(100).HasConversion<string>();
            builder.Property(s => s.CreatedAt).IsRequired();
            builder.Property(s => s.CreatedBy).HasMaxLength(255);
            builder.Property(s => s.UpdatedAt);
            builder.Property(s => s.UpdatedBy).HasMaxLength(255);
            builder.Property(s => s.SupportDelete).HasDefaultValueSql("false");

          
        }
    }
}
