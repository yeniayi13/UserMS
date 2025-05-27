using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Commoon.Dtos.Users.Response.Role_Permission;

namespace UserMs.Application.Queries.Roles_Permission
{
    public class GetRolesPermissionByIdQuery : IRequest<GetRolePermissionDto>
    {
        public Guid RolePermissionId { get; set; }

        public GetRolesPermissionByIdQuery(Guid rolePermissionId)
        {
            RolePermissionId = rolePermissionId;
        }
    }
}
