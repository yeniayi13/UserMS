using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Commoon.Dtos.Users.Request.User;
using UserMs.Commoon.Dtos.Users.Request.UserRole;
using UserMs.Commoon.Dtos.Users.Response.UserRole;
using UserMs.Domain.Entities;
using UserMs.Domain.User_Roles.ValueObjects;

namespace UserMs.Application.Commands.UsersRoles
{
    public class CreateUserRolesCommand : IRequest<UserRoleId>
    {
        public CreateUserRolesDto UsersRoles { get; set; }

        public CreateUserRolesCommand(CreateUserRolesDto usersRoles)
        {
            UsersRoles = usersRoles;
        }
    }
}
