using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Domain.Entities.Role.ValueObjects;

namespace UserMs.Domain.Entities.Role.ValueConverter
{
    public  class RoleConverter
    {
        public class RoleIdValueConverter : ValueConverter<RoleId, Guid>
        {
            public RoleIdValueConverter() : base(
                v => v.Value, // Convierte UserId a Guid
                v => RoleId.Create(v) // Convierte Guid a UserId
            )
            { }
        }

        public class RoleNameValueConverter : ValueConverter<RoleName, string>
        {
            public RoleNameValueConverter() : base(
                v => v.Value, // Convierte UserId a Guid
                v => RoleName.Create(v) // Convierte Guid a UserId
            )
            { }
        }

        public class RoleDeleteConverter : ValueConverter<RoleDelete, bool>
        {
            public RoleDeleteConverter() : base(
                v => v.Value, // Convierte UserId a Guid
                v => RoleDelete.Create(v) // Convierte Guid a UserId
            )
            { }
        }


    }
}
