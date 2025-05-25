using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Commoon.Dtos.Users.Request.UserRole;
using UserMs.Commoon.Dtos.Users.Response.Role;
using UserMs.Commoon.Dtos.Users.Response.User;

namespace UserMs.Commoon.Dtos.Users.Response.UserRole
{
    public class GetUserRoleDto
    {
        public Guid UserRoleId { get; set; } // ID del documento en MongoDB
        public Guid UserId { get; set; }
        public Guid RoleId { get; set; }
        public string UserEmail { get; set; } // ID del usuario
        public string RoleName { get; set; } // ID del rol
     

    }
}
