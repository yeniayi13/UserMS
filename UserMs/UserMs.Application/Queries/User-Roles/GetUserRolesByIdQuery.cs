using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Commoon.Dtos.Users.Response.UserRole;

namespace UserMs.Application.Queries.User_Roles
{
    public class GetUserRolesByIdByUserIDQuery : IRequest<GetUserRoleDto>
    {
        public Guid UserRoleId { get; set; }

        public GetUserRolesByIdByUserIDQuery(Guid userRoleId)
        {
            UserRoleId = userRoleId;
        }
    }
}
