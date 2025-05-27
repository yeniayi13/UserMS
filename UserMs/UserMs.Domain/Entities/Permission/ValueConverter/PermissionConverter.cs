using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Domain.Entities.Permission.ValueObjects;
using UserMs.Domain.Entities.Role.ValueObjects;

namespace UserMs.Domain.Entities.Permission.ValueConverter
{
    public class PermissionConverter
    {
        public class PermissionIdValueConverter : ValueConverter<PermissionId, Guid>
        {
            public PermissionIdValueConverter() : base(
                v => v.Value, // Convierte UserId a Guid
                v => PermissionId.Create(v) // Convierte Guid a UserId
            )
            { }
        }

        public class PermissionNameValueConverter : ValueConverter<PermissionName, string>
        {
            public PermissionNameValueConverter() : base(
                v => v.Value, // Convierte UserId a Guid
                v => PermissionName.Create(v) // Convierte Guid a UserId
            )
            { }
        }
    }
}
