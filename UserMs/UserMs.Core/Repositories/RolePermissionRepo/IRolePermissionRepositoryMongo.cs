using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Commoon.Dtos.Users.Response.Role_Permission;

namespace UserMs.Core.Repositories.RolePermissionRepo
{
    public interface IRolePermissionRepositoryMongo
    {
        Task<List<GetRolePermissionDto>> GetRolesPermissionAsync();
        Task<GetRolePermissionDto?> GetRolesPermissionByRoleQuery(string roleName);
        Task<GetRolePermissionDto?> GetRolesPermissionByIdQuery(Guid rolePermissionId);

        Task<GetRolePermissionDto?> GetByRoleAndPermissionAsync(Guid roleId, Guid permissionId);
    }
}
