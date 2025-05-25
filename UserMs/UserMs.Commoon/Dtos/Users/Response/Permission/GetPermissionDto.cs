using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserMs.Commoon.Dtos.Users.Response.Permission
{
    public class GetPermissionDto
    {
        public Guid PermissionId { get; set; }
        public string PermissionName { get; set; }
       
    }
}
