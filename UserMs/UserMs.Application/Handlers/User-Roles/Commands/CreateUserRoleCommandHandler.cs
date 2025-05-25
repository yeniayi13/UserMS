using AutoMapper;
using MediatR;
using Newtonsoft.Json;
using UserMs.Application.Commands.UsersRoles;
using UserMs.Commoon.Dtos;
using UserMs.Commoon.Dtos.Users.Request.UserRole;
using UserMs.Commoon.Dtos.Users.Response.ActivityHistory;
using UserMs.Commoon.Dtos.Users.Response.Role;
using UserMs.Commoon.Dtos.Users.Response.User;
using UserMs.Commoon.Dtos.Users.Response.UserRole;
using UserMs.Core;
using UserMs.Core.RabbitMQ;
using UserMs.Core.Repositories.ActivityHistoryRepo;
using UserMs.Core.Repositories.RolesRepo;
using UserMs.Core.Repositories.UserRepo;
using UserMs.Core.Repositories.UserRoleRepo;
using UserMs.Core.Service.Keycloak;
using UserMs.Domain.User_Roles;
using UserMs.Domain.User_Roles.ValueObjects;
using UserMs.Infrastructure.RabbitMQ.Consumer;

namespace UserMs.Application.Handlers.User_Roles.Commands
{
    public class CreateUserRoleCommandHandler : IRequestHandler<CreateUserRolesCommand, UserRoleId>
    {
        
        private readonly IUserRoleRepository _userRoleRepository;
        private readonly IUserRoleRepositoryMongo _userRoleRepositoryMongo;
        private readonly IEventBus<GetUserRoleDto> _eventBus;
        private readonly IUserRepository _usersRepository;
        private readonly IUserRepositoryMongo _usersRepositoryMongo;
        private readonly IRolesRepository _roleRepository;
        private readonly IKeycloakService _keycloakMsService;
        private readonly IActivityHistoryRepository _activityHistoryRepository;
        private readonly IEventBus<GetActivityHistoryDto> _eventBusActivity;
        private readonly IMapper _mapper;

        public CreateUserRoleCommandHandler(
            IMapper mapper,
            IEventBus<GetActivityHistoryDto> eventBusActivity,
            IActivityHistoryRepository activityHistoryRepository,
            IKeycloakService keycloakMsService,
            IUserRepository usersRepository,
            IUserRepositoryMongo usersRepositoryMongo,
            IRolesRepository roleRepository,
            IUserRoleRepository userRoleRepository,
            IUserRoleRepositoryMongo userRoleRepositoryMongo,
            IEventBus<GetUserRoleDto> eventBus)
        {
            _mapper = mapper;
            _eventBusActivity = eventBusActivity;
            _activityHistoryRepository = activityHistoryRepository;
            _keycloakMsService = keycloakMsService;
            _usersRepository = usersRepository;
            _usersRepositoryMongo = usersRepositoryMongo;
            _roleRepository = roleRepository;
            _userRoleRepository = userRoleRepository;
            _userRoleRepositoryMongo = userRoleRepositoryMongo;
            _eventBus = eventBus;
        }


        public async Task<UserRoleId> Handle(CreateUserRolesCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var userRoleId = UserRoleId.Create();
                var userId = request.UsersRoles.UserId;
                var roleId = request.UsersRoles.RoleId;

                var user = await _usersRepositoryMongo.GetUsersById(userId);
                var role = await _roleRepository.GetRolesByIdQuery(roleId);

                if (user == null || role == null)
                {
                    throw new InvalidOperationException("Error: No se encontró el usuario o el rol.");
                }

                // 🔹 Verificar si el usuario ya tiene el rol asignado
                var roleUser =
                    await _userRoleRepositoryMongo.GetRoleByIdAndByUserIdQuery(role.RoleName.Value, user.UserEmail.Value);
                if (roleUser != null)
                {
                  
                    return roleUser.UserRoleId; // ⚠️ En lugar de lanzar una excepción, retorna el ID existente.
                }

                var userRole = new UserRoles(userRoleId, userId, roleId);

                // 🔹 Guardar en PostgreSQL
                await _userRoleRepository.AddAsync(userRole);

                // 🔹 Construir el evento con los datos completos
                var data = new GetUserRoleDto
                {
                    UserRoleId = userRole.UserRoleId,
                    UserId = user.UserId,
                    RoleId = role.RoleId,
                    UserEmail = user.UserEmail.Value,
                    RoleName = role.RoleName.Value
                };

                await _keycloakMsService.AssignClientRoleToUser(userId, data.RoleName);
                await _eventBus.PublishMessageAsync(data, "userRoleQueue", "USER_ROLE_CREATED");

               

                var activity = new Domain.Entities.ActivityHistory.ActivityHistory(
                    Guid.NewGuid(),
                    userId,
                    "Asignó un rol a usuario",
                    DateTime.UtcNow
                );
                await _activityHistoryRepository.AddAsync(activity);
                var activityDto = _mapper.Map<GetActivityHistoryDto>(activity);
                await _eventBusActivity.PublishMessageAsync(activityDto, "activityHistoryQueue", "ACTIVITY_CREATED");

                return userRole.UserRoleId;
            }
            catch (InvalidOperationException ex)
            {
              
                throw;
            }
            catch (Exception ex)
            {
                
                throw new ApplicationException("Ocurrió un error inesperado al asignar un rol al usuario.", ex);
            }
        }
    }
    
}
