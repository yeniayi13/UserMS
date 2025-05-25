using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserMs.Domain.Entities.Permission;

namespace UserMs.Infrastructure.Database.Configuration.Postgres
{
    [ExcludeFromCodeCoverage]
    public class PermissionConfiguration : IEntityTypeConfiguration<Permissions>
    {
        public void Configure(EntityTypeBuilder<Permissions> builder)
        {
            builder.HasKey(p => p.PermissionId);
            builder.Property(p => p.PermissionName).IsRequired().HasMaxLength(100);

            // Configuración de la relación con RolePermissionEntity
            builder.HasMany(p => p.RolePermission)
                .WithOne()
                .HasForeignKey(rp => rp.PermissionId);

            // Auditoría
            builder.Property(p => p.CreatedAt).IsRequired();
            builder.Property(p => p.CreatedBy).HasMaxLength(50);
            builder.Property(p => p.UpdatedAt);
            builder.Property(p => p.UpdatedBy).HasMaxLength(50);
        }
    }
}
