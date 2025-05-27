using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Domain.Entities;
using UserMs.Domain.Entities.Role_Permission.ValueObjects;

namespace UserMs.Application.Commands.RolesPermission
{
    public class DeleteRolePermissionCommand : IRequest<RolePermissionId>
    {
        public RolePermissionId RolePermissionId { get; set; }
        public DeleteRolePermissionCommand(RolePermissionId rolePermissionId)
        {
            RolePermissionId = rolePermissionId;
        }
    }
}
