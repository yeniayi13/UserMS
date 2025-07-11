using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Commoon.Dtos.Users.Request.RolePermission;
using UserMs.Commoon.Dtos.Users.Request.User;
using UserMs.Domain.Entities;
using UserMs.Domain.Entities.Role_Permission.ValueObjects;

namespace UserMs.Application.Commands.RolesPermission
{
    public class CreateRolePermissionCommand : IRequest<Guid>
    {
        public CreateRolePermissionDto RolePermission { get; set; }

        public CreateRolePermissionCommand(CreateRolePermissionDto rolePermission)
        {
            RolePermission = rolePermission;
           // RolePermission = rolePermission;
        }
    }
}
