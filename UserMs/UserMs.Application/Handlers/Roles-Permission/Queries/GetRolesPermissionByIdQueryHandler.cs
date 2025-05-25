using AutoMapper;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Application.Queries.Roles_Permission;
using UserMs.Commoon.Dtos.Users.Response.Role_Permission;
using UserMs.Core.Repositories.ActivityHistoryRepo;
using UserMs.Core.Repositories.RolePermissionRepo;

namespace UserMs.Application.Handlers.Roles_Permission.Queries
{
    public class GetRolesPermissionByIdQueryHandler : IRequestHandler<GetRolesPermissionByIdQuery, GetRolePermissionDto>
    {
        private readonly IRolePermissionRepositoryMongo _rolePermissionRepository;
        private readonly IMapper _mapper;
        private readonly IActivityHistoryRepository _activityHistoryRepository;
        public GetRolesPermissionByIdQueryHandler(IRolePermissionRepositoryMongo rolePermissionRepository, IMapper mapper)
        {
            _rolePermissionRepository = rolePermissionRepository;
            _mapper = mapper;
        }

        public async Task<GetRolePermissionDto> Handle(GetRolesPermissionByIdQuery request, CancellationToken cancellationToken)
        {
            var rolePermission = await _rolePermissionRepository.GetRolesPermissionByIdQuery(request.RolePermissionId);
            // if (rolePermission == null) 
            // throw new RolePermissionNotFoundException("Role permission not found.");

            var rolePermissionDto = _mapper.Map<GetRolePermissionDto>(rolePermission);
            return rolePermissionDto;
        }
    }
}
