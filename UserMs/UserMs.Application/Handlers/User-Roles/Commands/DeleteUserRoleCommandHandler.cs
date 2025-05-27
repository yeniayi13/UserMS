using AutoMapper;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Application.Commands.UsersRoles;
using UserMs.Commoon.Dtos.Users.Response.ActivityHistory;
using UserMs.Commoon.Dtos.Users.Response.UserRole;
using UserMs.Core;
using UserMs.Core.RabbitMQ;
using UserMs.Core.Repositories.ActivityHistoryRepo;
using UserMs.Core.Repositories.UserRepo;
using UserMs.Core.Repositories.UserRoleRepo;
using UserMs.Core.Service.Keycloak;
using UserMs.Domain.Entities;
using UserMs.Domain.Entities.UserEntity;
using UserMs.Domain.User_Roles.ValueObjects;
using UserMs.Infrastructure.Exceptions;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace UserMs.Application.Handlers.User_Roles.Commands
{
    public class DeleteUserRoleCommandHandler : IRequestHandler<DeleteUserRolesCommand, Guid>
    {
        private readonly IUserRoleRepository _userRoleRepository;
        private readonly IEventBus<GetUserRoleDto> _eventBus;
        private readonly IMapper _mapper;
        private readonly IKeycloakService _keycloakMsService;
        private readonly IUserRepository _usersRepository;
        private readonly IActivityHistoryRepository _activityHistoryRepository;
        private readonly IEventBus<GetActivityHistoryDto> _eventBusActivity;
        private readonly IUserRoleRepositoryMongo _userRoleRepositoryMongo;
        public DeleteUserRoleCommandHandler(
            IEventBus<GetActivityHistoryDto> eventBusActivity,
            IActivityHistoryRepository activityHistoryRepository,
            IUserRepository usersRepository,
            IUserRoleRepositoryMongo userRoleRepositoryMongo,
            IKeycloakService keycloakMsService,
            IUserRoleRepository userRoleRepository,
            IEventBus<GetUserRoleDto> eventBus,
            IMapper mapper)
        {
            _userRoleRepository = userRoleRepository;
            _userRoleRepositoryMongo = userRoleRepositoryMongo;
            _eventBus = eventBus;
            _mapper = mapper;
            _keycloakMsService = keycloakMsService;
            _usersRepository = usersRepository;
            _activityHistoryRepository = activityHistoryRepository;
            _eventBusActivity = eventBusActivity;
        }


        public async Task<Guid> Handle(DeleteUserRolesCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var userRole = await _userRoleRepositoryMongo.GetRoleByIdAndByUserIdQuery(request.Rol, request.Email);

                if (userRole == null)
                    throw new UserNotFoundException("Error: No se encontró el usuario con el rol especificado.");

                var userRoleDto = _mapper.Map<GetUserRoleDto>(userRole);

                await _userRoleRepository.DeleteUsersRoleAsync(userRoleDto.UserRoleId);
                await _eventBus.PublishMessageAsync(userRoleDto, "userRoleQueue", "USER_ROLE_DELETED");
                await _keycloakMsService.RemoveClientRoleFromUser(userRole.UserId, request.Rol);

                var activity = new Domain.Entities.ActivityHistory.ActivityHistory(
                    Guid.NewGuid(),
                    userRoleDto.UserId,
                    "Eliminó un rol a usuario",
                    DateTime.UtcNow
                );
                await _activityHistoryRepository.AddAsync(activity);
                var activityDto = _mapper.Map<GetActivityHistoryDto>(activity);
                await _eventBusActivity.PublishMessageAsync(activityDto, "activityHistoryQueue", "ACTIVITY_CREATED");

                return userRole.UserId;
            }
            catch (Exception ex)
            {
                ExceptionHandlerService.HandleException(ex);
                throw;
            }
        }
    }
}
