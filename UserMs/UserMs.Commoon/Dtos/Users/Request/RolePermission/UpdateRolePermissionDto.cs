using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserMs.Commoon.Dtos.Users.Request.RolePermission
{
    public class UpdateRolePermissionDto
    {
        public Guid RoleId { get; set; }
        public Guid PermissionId { get; set; }
    }
}
