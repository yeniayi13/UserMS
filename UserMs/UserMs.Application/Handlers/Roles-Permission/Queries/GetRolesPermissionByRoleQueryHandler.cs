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
    public class GetRolesPermissionByRoleQueryHandler : IRequestHandler<GetRolesPermissionByRoleQuery, List<GetRolePermissionDto>>
    {
        private readonly IRolePermissionRepositoryMongo _rolePermissionRepository;
        private readonly IMapper _mapper;
        private readonly IActivityHistoryRepository _activityHistoryRepository;
        public GetRolesPermissionByRoleQueryHandler(IRolePermissionRepositoryMongo rolePermissionRepository, IMapper mapper)
        {
            _rolePermissionRepository = rolePermissionRepository;
            _mapper = mapper;
        }

        public async Task<List<GetRolePermissionDto>> Handle(GetRolesPermissionByRoleQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var rolePermissions = await _rolePermissionRepository.GetRolesPermissionByRoleQuery(request.RoleName);

                if (rolePermissions == null )
                {
                    Console.WriteLine($"No se encontraron permisos para el rol con ID: {request.RoleName}");
                    return new List<GetRolePermissionDto>(); // Retornar lista vacía en lugar de `null`
                }

                var rolePermissionsDto = _mapper.Map<List<GetRolePermissionDto>>(rolePermissions);
                return rolePermissionsDto;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en Handle(): {ex.Message}");
                return new List<GetRolePermissionDto>(); // Retornar lista vacía en caso de error
            }
        }
    }
}
