using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Commoon.Dtos.Users.Response.Role_Permission;
using UserMs.Commoon.Dtos.Users.Response.UserRole;

namespace UserMs.Application.Queries.Roles_Permission
{
    public class GetRolesPermissionsAllQuery : IRequest<List<GetRolePermissionDto>>
    {
        public Guid RolesPermissionId { get; set; }
        public Guid PermissionId { get; set; }
        public Guid RoleId { get; set; }


    }
}
