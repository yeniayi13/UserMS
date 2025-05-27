using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Commoon.Dtos.Users.Response.UserRole;

namespace UserMs.Application.Queries.User_Roles
{
    public class GetUserRolesByRoleNameQuery : IRequest<List<GetUserRoleDto>>
    {
        public string RoleName { get; set; }

        public GetUserRolesByRoleNameQuery(string roleName)
        {
            RoleName = roleName;
        }
    }
}
