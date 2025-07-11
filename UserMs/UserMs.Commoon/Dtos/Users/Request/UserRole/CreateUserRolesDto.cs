using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using UserMs.Domain.Entities;
using UserMs.Domain.User_Roles.ValueObjects;

namespace UserMs.Commoon.Dtos.Users.Request.UserRole
{
    public class CreateUserRolesDto
    {

        [JsonIgnore]
        public Guid UserRoleId { get;  set; }
        public Guid UserId { get;  set; }
        public Guid RoleId { get;  set; }

    }
}
