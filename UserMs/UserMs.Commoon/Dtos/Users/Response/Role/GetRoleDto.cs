using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserMs.Commoon.Dtos.Users.Response.Role
{
    public class GetRoleDto
    {
        public Guid RoleId { get; set; }
        public string RoleName { get; set; } = String.Empty;

    }
}
