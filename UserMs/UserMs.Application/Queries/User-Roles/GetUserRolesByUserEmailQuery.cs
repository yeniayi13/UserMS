using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Commoon.Dtos.Users.Response.User;
using UserMs.Commoon.Dtos.Users.Response.UserRole;

namespace UserMs.Application.Queries.User_Roles
{
    public class GetUserRolesByUserEmailQuery : IRequest <List<GetUserRoleDto>>
    {
        public string UserEmail { get; set; }

        public GetUserRolesByUserEmailQuery(string userEmail)
        {
            UserEmail = userEmail;
        }
    }
}
