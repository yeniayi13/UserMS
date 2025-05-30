using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Domain.Entities.Permission.ValueObjects;
using UserMs.Domain.Entities.Role_Permission;

namespace UserMs.Domain.Entities.Permission
{
    public  class Permissions
    {
        
        public PermissionId PermissionId { get;  set; }
        public PermissionName PermissionName { get;  set; }

        public ICollection<RolePermissions> RolePermission { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }


        public Permissions(PermissionId permissionId, PermissionName permissionName)
        {
            PermissionId = permissionId;
            PermissionName = permissionName;
           
        }



    }
}
