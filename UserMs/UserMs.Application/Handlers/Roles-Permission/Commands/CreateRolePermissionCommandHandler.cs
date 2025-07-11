using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Application.Commands.RolesPermission;
using UserMs.Commoon.Dtos.Users.Request.RolePermission;
using UserMs.Commoon.Dtos.Users.Response.Role_Permission;
using UserMs.Commoon.Dtos.Users.Response.UserRole;
using UserMs.Core.RabbitMQ;
using UserMs.Core.Repositories.ActivityHistoryRepo;
using UserMs.Core.Repositories.PermissionRepo;
using UserMs.Core.Repositories.RolePermissionRepo;
using UserMs.Core.Repositories.RolesRepo;
using UserMs.Domain.Entities.Role_Permission;
using UserMs.Domain.Entities.Role_Permission.ValueObjects;
using UserMs.Domain.Entities.UserEntity;
using UserMs.Infrastructure.Exceptions;

namespace UserMs.Application.Handlers.Roles_Permission.Commands
{
    public class CreateRolePermissionCommandHandler : IRequestHandler<CreateRolePermissionCommand, Guid>
    {
        private readonly IRolePermissionRepository _rolePermissionRepository;
        private readonly IRolesRepository _roleRepository;
        private readonly IRolePermissionRepositoryMongo _roleRepositoryMongo;
        private readonly IPermissionRepositoryMongo _permissionRepository;
        private readonly IEventBus<GetRolePermissionDto> _eventBus;
        private readonly IActivityHistoryRepository _activityHistoryRepository;

        public CreateRolePermissionCommandHandler(IRolePermissionRepositoryMongo roleRepositoryMongo,IRolesRepository roleRepository, IPermissionRepositoryMongo permissionRepository,IRolePermissionRepository rolePermissionRepository, IEventBus<GetRolePermissionDto> eventBus)
        {
            _rolePermissionRepository = rolePermissionRepository;
            _eventBus = eventBus;
            _permissionRepository = permissionRepository;
            _roleRepository = roleRepository;
            _roleRepositoryMongo = roleRepositoryMongo;
        }

        public async Task<Guid> Handle(CreateRolePermissionCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var rolePermissionId = RolePermissionId.Create();
                var roleId = request.RolePermission.RoleId;
                var permissionId = request.RolePermission.PermissionId;

                // Verificar si el permiso y el rol existen
                var permissions = await _permissionRepository.GetPermissionByIdAsync(permissionId);
                var role = await _roleRepository.GetRolesByIdQuery(roleId);

                if (permissions == null || role == null)
                {
                    throw new InvalidOperationException("Error: No se encontró el permiso o el rol.");
                }

                // **Validar que la combinación no exista**
                var existingRolePermission = await _roleRepositoryMongo.GetByRoleAndPermissionAsync(roleId, permissionId);
                if (existingRolePermission != null)
                {
                    throw new InvalidOperationException("Error: La combinación de rol y permiso ya existe.");
                }

                var rolePermission = new RolePermissions(
                    rolePermissionId,
                    roleId,
                    permissionId
                );

                var data = new GetRolePermissionDto
                {
                    RolePermissionId = rolePermissionId.Value,
                    PermissionId = permissions.PermissionId.Value,
                    PermissionName = permissions.PermissionName.Value,
                    RoleId = role.RoleId.Value,
                    RoleName = role.RoleName.Value
                };

                await _rolePermissionRepository.AddAsync(rolePermission);
                await _eventBus.PublishMessageAsync(data, "rolePermissionQueue", "ROLE_PERMISSION_CREATED");

                return rolePermission.RolePermissionId.Value;
            }
            catch (InvalidOperationException ex)
            {
                
                throw;
            }
            catch (Exception ex)
            {
               // ExceptionHandlerService.HandleException(ex);
                throw;
            }
        }
    }
}
