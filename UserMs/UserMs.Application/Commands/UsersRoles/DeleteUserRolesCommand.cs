using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Domain.Entities.Role.ValueObjects;
using UserMs.Domain.User_Roles.ValueObjects;

namespace UserMs.Application.Commands.UsersRoles
{
    public class DeleteUserRolesCommand : IRequest<Guid>
    {
        public string Rol { get; set; }
        public string Email { get; set; }

        //public  UserRoleId  UserRoleId { get; set; }
        public DeleteUserRolesCommand(string rol, string email)
        {
            Rol = rol;
            Email = email;
           // UserRoleId = userRoleId;
        }
    }
}
