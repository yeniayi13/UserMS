using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserMs.Commoon.Dtos.Users.Request.UserRole
{
    public class UpdateUserRolesDto
    {
        public Guid UserId { get;  set; }
        public Guid RoleId { get;  set; }
    }
}
