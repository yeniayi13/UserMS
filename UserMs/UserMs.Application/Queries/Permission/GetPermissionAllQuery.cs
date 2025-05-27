using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Commoon.Dtos.Users.Response.Permission;
using UserMs.Commoon.Dtos.Users.Response.Role;

namespace UserMs.Application.Queries.Permission
{
    public class GetPermissionAllQuery : IRequest<List<GetPermissionDto>>
    {

        public Guid PermissionId { get; set; }
        public string PermissionName { get; set; }


    }
}
