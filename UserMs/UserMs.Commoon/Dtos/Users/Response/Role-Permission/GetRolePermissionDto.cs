using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.JavaScript;
using System.Text;
using System.Threading.Tasks;

namespace UserMs.Commoon.Dtos.Users.Response.Role_Permission
{
    public class GetRolePermissionDto
    {
        
        public Guid RolePermissionId { get; set; }
        public Guid RoleId { get; set; }
        public Guid PermissionId { get; set; }

        public string RoleName { get; set; } 
        public string PermissionName { get; set; }

       
    }
    
}
