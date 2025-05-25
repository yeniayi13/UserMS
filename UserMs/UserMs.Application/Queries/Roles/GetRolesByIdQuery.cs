using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Commoon.Dtos.Users.Response.Role;
using UserMs.Commoon.Dtos.Users.Response.UserRole;

namespace UserMs.Application.Queries.Roles
{
    public class GetRolesByIdQuery : IRequest<GetRoleDto>
    {
        public Guid RoleId { get; set; }

        public GetRolesByIdQuery(Guid roleId)
        {
            RoleId = roleId;
        }
    }
}
