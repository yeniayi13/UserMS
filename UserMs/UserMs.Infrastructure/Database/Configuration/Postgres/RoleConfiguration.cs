using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserMs.Domain.Entities.Role;

namespace UserMs.Infrastructure.Database.Configuration.Postgres
{
    [ExcludeFromCodeCoverage]
    public class RoleConfiguration : IEntityTypeConfiguration<Roles>
    {
        public void Configure(EntityTypeBuilder<Roles> builder)
        {
            builder.HasKey(r => r.RoleId);
            builder.Property(r => r.RoleName).IsRequired().HasMaxLength(100);
            builder.Property(r => r.IsDeleted).HasDefaultValueSql("false");
            builder.Property(p => p.CreatedAt).IsRequired();
            builder.Property(p => p.CreatedBy).HasMaxLength(50);
            builder.Property(p => p.UpdatedAt);
            builder.Property(p => p.UpdatedBy).HasMaxLength(50);

            // Configuración de la relación con RolePermissionEntity
            builder.HasMany(r => r.RolePermission)
                .WithOne()
                .HasForeignKey(rp => rp.RoleId);

            builder.HasMany(r => r.UserRoles)
                .WithOne(ur => ur.Role)
                .HasForeignKey(ur => ur.RoleId);
        }
    }
}
