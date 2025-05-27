using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Domain.Entities.Role.ValueObjects;
using UserMs.Domain.Entities.Role_Permission.ValueObjects;

namespace UserMs.Domain.Entities.Role_Permission.ValueConverter
{
    public class RolePermissionConverter
    {
        public class RolePermissionIdValueConverter : ValueConverter<RolePermissionId, Guid>
        {
            public RolePermissionIdValueConverter() : base(
                v => v.Value, // Convierte UserId a Guid
                v => RolePermissionId.Create(v) // Convierte Guid a UserId
            )
            { }
        }
    }
}
