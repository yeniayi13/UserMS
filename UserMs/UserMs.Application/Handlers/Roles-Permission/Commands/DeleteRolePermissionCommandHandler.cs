using AutoMapper;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Application.Commands.RolesPermission;
using UserMs.Commoon.Dtos.Users.Response.Role_Permission;
using UserMs.Core.RabbitMQ;
using UserMs.Core.Repositories.RolePermissionRepo;
using UserMs.Domain.Entities.Role_Permission.ValueObjects;
using UserMs.Domain.Entities;
using UserMs.Core.Repositories.ActivityHistoryRepo;

namespace UserMs.Application.Handlers.Roles_Permission.Commands
{
    public class DeleteRolePermissionCommandHandler : IRequestHandler<DeleteRolePermissionCommand, RolePermissionId>
    {
        private readonly IRolePermissionRepository _rolePermissionRepository;
        private readonly IRolePermissionRepositoryMongo _rolePermissionRepositoryMongo;
        private readonly IEventBus<GetRolePermissionDto> _eventBus;
        private readonly IMapper _mapper;
        private readonly IActivityHistoryRepository _activityHistoryRepository;
        public DeleteRolePermissionCommandHandler(IRolePermissionRepositoryMongo rolePermissionRepositoryMongo,IEventBus<GetRolePermissionDto> eventBus,IRolePermissionRepository rolePermissionRepository, IMapper mapper)
        {
            _rolePermissionRepository = rolePermissionRepository;
            _eventBus = eventBus;
            _mapper = mapper;
            _rolePermissionRepositoryMongo = rolePermissionRepositoryMongo;
        }

        public async Task<RolePermissionId> Handle(DeleteRolePermissionCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var rolePermission = await _rolePermissionRepositoryMongo.GetRolesPermissionByIdQuery(request.RolePermissionId);

                if (rolePermission == null)
                    throw new InvalidOperationException("Error: No se encontró el permiso de rol.");

                await _rolePermissionRepository.DeleteRolePermissionAsync(rolePermission.RolePermissionId);
                var rolePermissionDto = _mapper.Map<GetRolePermissionDto>(rolePermission);
                await _eventBus.PublishMessageAsync(rolePermissionDto, "rolePermissionQueue", "ROLE_PERMISSION_DELETED");

                return request.RolePermissionId;
            }
            catch (InvalidOperationException ex)
            {
                
                throw;
            }
            catch (Exception ex)
            {
                
                throw new ApplicationException("Ocurrió un error al eliminar el permiso de rol.", ex);
            }
        }
    }
}
