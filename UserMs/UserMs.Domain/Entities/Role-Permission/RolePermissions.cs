using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Domain.Entities.Permission;
using UserMs.Domain.Entities.Permission.ValueObjects;
using UserMs.Domain.Entities.Role;
using UserMs.Domain.Entities.Role.ValueObjects;
using UserMs.Domain.Entities.Role_Permission.ValueObjects;

namespace UserMs.Domain.Entities.Role_Permission
{
    public  class RolePermissions
    {
       
        public RolePermissionId RolePermissionId { get;  set; } 
        public RoleId RoleId { get;  set; }
        public PermissionId PermissionId { get;  set; }

       
        public Roles Role { get;  set; }
        public Permissions Permission { get;  set; }

        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
        public RolePermissions() { }
       


        public RolePermissions(RolePermissionId rolePermissionId ,RoleId roleId, PermissionId permissionId)
        {
            RolePermissionId = rolePermissionId;
            RoleId = roleId;
            PermissionId = permissionId;
        }

        public RolePermissions( RoleId roleId, PermissionId permissionId)
        {
       
            RoleId = roleId;
            PermissionId = permissionId;
        }

    }
}
