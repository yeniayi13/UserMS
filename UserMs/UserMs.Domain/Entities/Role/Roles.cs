using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Domain.Entities.Role_Permission;
using UserMs.Domain.Entities.Role.ValueObjects;
using UserMs.Domain.User_Roles;

namespace UserMs.Domain.Entities.Role
{
    public  class Roles
    {
        public RoleId RoleId { get;  set; }
        public RoleName RoleName { get;  set; }
        public ICollection<UserRoles> UserRoles { get; set; }

        public RoleDelete IsDeleted { get; private set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
        public ICollection<RolePermissions> RolePermission { get; set; }

        public Roles(RoleId roleId, RoleName roleName, RoleDelete isDeleted)
        {
            RoleId = roleId;
            RoleName = roleName;
            IsDeleted = isDeleted;
        }

        public Roles()
        {
        }
        public void SetRoleDelete(RoleDelete isDeleted)
        {
            IsDeleted = isDeleted;
        }
    }
}
