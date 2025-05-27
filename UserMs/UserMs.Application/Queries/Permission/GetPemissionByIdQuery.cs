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
    public class GetPemissionByIdQuery : IRequest<GetPermissionDto>
    {
        public Guid PermissionId { get; set; }

        public GetPemissionByIdQuery(Guid permissionId)
        {
            PermissionId = permissionId;
        }
    }
}
