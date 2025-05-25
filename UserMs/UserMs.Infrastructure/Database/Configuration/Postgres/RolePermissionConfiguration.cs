using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserMs.Domain.Entities.Role_Permission;

namespace UserMs.Infrastructure.Database.Configuration.Postgres
{
    [ExcludeFromCodeCoverage]
    public class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermissions>
    {
        public void Configure(EntityTypeBuilder<RolePermissions> builder)
        {
            builder.HasKey(r => r.RolePermissionId);

            // Configuración de las relaciones
            builder.HasOne(rp => rp.Role)
                .WithMany(r => r.RolePermission)
                .HasForeignKey(rp => rp.RoleId);

            builder.HasOne(rp => rp.Permission)
                .WithMany(p => p.RolePermission)
                .HasForeignKey(rp => rp.PermissionId);

            // Auditoría
            builder.Property(rp => rp.CreatedAt).IsRequired();
            builder.Property(rp => rp.CreatedBy).HasMaxLength(50);
            builder.Property(rp => rp.UpdatedAt);
            builder.Property(rp => rp.UpdatedBy).HasMaxLength(50);
        }
    }
}
