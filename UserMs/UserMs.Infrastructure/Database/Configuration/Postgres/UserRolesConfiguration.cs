using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Domain.User_Roles;

namespace UserMs.Infrastructure.Database.Configuration.Postgres
{
    [ExcludeFromCodeCoverage]
    public class UserRolesConfiguration : IEntityTypeConfiguration<UserRoles>
    {
        public void Configure(EntityTypeBuilder<UserRoles> builder)
        {
            // Definir clave primaria
            builder.HasKey(ur => ur.UserRoleId);

            // Configurar relación con Users
            builder.HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId);

            // Configurar relación con Roles
            builder.HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId);

            // Configurar propiedades de auditoría
            builder.Property(ur => ur.CreatedAt).IsRequired();
            builder.Property(ur => ur.CreatedBy).HasMaxLength(50);
            builder.Property(ur => ur.UpdatedAt);
            builder.Property(ur => ur.UpdatedBy).HasMaxLength(50);
        }
    }
}
