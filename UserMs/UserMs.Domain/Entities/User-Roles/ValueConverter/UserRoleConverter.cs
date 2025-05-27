using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Domain.Entities.Role_Permission.ValueObjects;
using UserMs.Domain.User_Roles.ValueObjects;

namespace UserMs.Domain.User_Roles.ValueConverter
{
    public class UserRoleConverter
    {
        public class UserRoleIdValueConverter : ValueConverter<UserRoleId, Guid>
        {
            public UserRoleIdValueConverter() : base(
                v => v.Value, // Convierte UserId a Guid
                v => UserRoleId.Create(v) // Convierte Guid a UserId
            )
            { }
        }
    }
}
