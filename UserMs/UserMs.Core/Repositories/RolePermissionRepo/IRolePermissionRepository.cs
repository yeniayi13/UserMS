using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Domain.Entities.UserEntity;
using UserMs.Domain.Entities;
using UserMs.Domain.User_Roles.ValueObjects;
using UserMs.Domain.User_Roles;
using UserMs.Domain.Entities.Role_Permission;
using UserMs.Domain.Entities.Role_Permission.ValueObjects;
using UserMs.Domain.Entities.Permission.ValueObjects;
using UserMs.Domain.Entities.Role.ValueObjects;
using UserMs.Commoon.Dtos.Users.Response.Role_Permission;

namespace UserMs.Core.Repositories.RolePermissionRepo
{
    public interface IRolePermissionRepository
    {
        
        Task AddAsync(RolePermissions rolePermissions);
        Task<RolePermissions?> UpdateRolePermissionAsync(RolePermissionId rolePermissionId, RolePermissions rolePermissions);
        Task<RolePermissions?> DeleteRolePermissionAsync(RolePermissionId rolePermissionId);

       
    }
}
