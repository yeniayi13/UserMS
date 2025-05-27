using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Domain.Entities;
using UserMs.Domain.Entities.Role;
using UserMs.Domain.Entities.Role.ValueObjects;
using UserMs.Domain.Entities.UserEntity;
using UserMs.Domain.User_Roles.ValueObjects;

namespace UserMs.Domain.User_Roles
{
    public class UserRoles
    {
       public UserRoleId UserRoleId { get; private set; } 
        public UserId UserId { get; private set; }
        public Users User { get; private set; }
        public RoleId RoleId { get; private set; }
        public Roles Role { get; private set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }


        public UserRoles() { }
        public UserRoles(UserId userId, RoleId roleId)
        {
            UserId = userId;
            RoleId = roleId;
        }

        public UserRoles(UserRoleId userRoleId,UserId userId, RoleId roleId)
        {
            UserRoleId = userRoleId;
            UserId = userId;
            RoleId = roleId;
        }
    }
}
