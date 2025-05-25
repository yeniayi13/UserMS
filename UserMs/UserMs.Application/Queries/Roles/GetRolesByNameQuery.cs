using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Commoon.Dtos.Users.Response.Role;

namespace UserMs.Application.Queries.Roles
{
    public class GetRolesByNameQuery : IRequest<GetRoleDto>
    {
        public string RoleName { get; set; }

        public GetRolesByNameQuery(string roleName)
        {
            RoleName = roleName;
        }
    }
}
