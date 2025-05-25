using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Commoon.Dtos.Users.Response.Role;
using UserMs.Commoon.Dtos.Users.Response.Role_Permission;

namespace UserMs.Application.Queries.Roles
{
    public class GetRolesAllQuery : IRequest<List<GetRoleDto>>
    {

        public Guid RoleId { get; set; }
        public string RoleName { get; set; } 


    }
}
