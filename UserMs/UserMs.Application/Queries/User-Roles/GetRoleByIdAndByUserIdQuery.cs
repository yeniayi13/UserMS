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
    public class GetRoleByIdAndByUserIdQuery : IRequest<bool>
    {
        public string RoleId { get; set; }
        public string UserId { get; set; }

        public GetRoleByIdAndByUserIdQuery(string roleId, string userId)
        {
            RoleId = roleId;
            UserId = userId;
        }
    }
}
