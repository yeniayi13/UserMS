using AutoMapper;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Application.Commands.Support;
using UserMs.Commoon.Dtos.Users.Response.Support;
using UserMs.Core.RabbitMQ;
using UserMs.Core.Repositories.SupportsRepo;
using UserMs.Domain.Entities.Support.ValueObjects;
using UserMs.Domain.Entities;
using UserMs.Commoon.Dtos.Users.Response.User;
using UserMs.Core.Repositories.UserRepo;
using UserMs.Core;
using UserMs.Infrastructure.Repositories;
using UserMs.Core.Repositories.ActivityHistoryRepo;
using UserMs.Commoon.Dtos.Users.Response.ActivityHistory;
using UserMs.Core.Service.Keycloak;
using UserMs.Infrastructure.Exceptions;
using UserMs.Core.Repositories.Supports;

namespace UserMs.Application.Handlers.Support.Command
{
    public class DeleteSupportCommandHandler : IRequestHandler<DeleteSupportCommand, UserId>
    {
        private readonly ISupportRepository _supportRepository;
        private readonly ISupportRepositoryMongo _supportRepositoryMongo;
        private readonly IEventBus<GetSupportDto> _eventBus;
        private readonly IMapper _mapper;
        private readonly IKeycloakService _keycloakMsService;
        private readonly IEventBus<GetUsersDto> _eventBusUser;
        private readonly IUserRepository _usersRepository;
        private readonly IUserRepositoryMongo _usersRepositoryMongo;
        private readonly IActivityHistoryRepository _activityHistoryRepository;
        private readonly IEventBus<GetActivityHistoryDto> _eventBusActivity;
        public DeleteSupportCommandHandler(
            IEventBus<GetActivityHistoryDto> eventBusActivity,
            IActivityHistoryRepository activityHistoryRepository,
            ISupportRepository supportRepository,
            ISupportRepositoryMongo supportRepositoryMongo,
            IUserRepository usersRepository,
            IUserRepositoryMongo usersRepositoryMongo,
            IKeycloakService keycloakMsService,
            IEventBus<GetUsersDto> eventBusUser,
            IEventBus<GetSupportDto> eventBus,
            IMapper mapper)
        {
            _supportRepository = supportRepository;
            _supportRepositoryMongo = supportRepositoryMongo;
            _eventBus = eventBus;
            _mapper = mapper;
            _keycloakMsService = keycloakMsService;
            _eventBusUser = eventBusUser;
            _usersRepository = usersRepository;
            _usersRepositoryMongo = usersRepositoryMongo;
            _activityHistoryRepository = activityHistoryRepository;
            _eventBusActivity = eventBusActivity;
        }


        public async Task<UserId> Handle(DeleteSupportCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var support = await _supportRepositoryMongo.GetSupportByIdAsync(request.SupportId);
                var users = await _usersRepositoryMongo.GetUsersById(request.SupportId);

                if (support == null)
                    throw new UserNotFoundException("Support not found.");

                support.SetSupportDelete(SupportDelete.Create(true));

                await _supportRepository.DeleteAsync(request.SupportId);
                await _keycloakMsService.DeleteUserAsync(support!.UserId);
                await _usersRepository.DeleteUsersAsync(request.SupportId);

                var supportDto = _mapper.Map<GetSupportDto>(support);
                await _eventBus.PublishMessageAsync(supportDto, "supportQueue", "SUPPORT_DELETED");

                var usersDto = _mapper.Map<GetUsersDto>(users);
                await _eventBusUser.PublishMessageAsync(usersDto, "userQueue", "USER_DELETED");

                var activity = new Domain.Entities.ActivityHistory.ActivityHistory(
                    Guid.NewGuid(),
                    usersDto.UserId,
                    "Eliminó un trabajador de soporte",
                    DateTime.UtcNow
                );
                await _activityHistoryRepository.AddAsync(activity);
                var activityDto = _mapper.Map<GetActivityHistoryDto>(activity);
                await _eventBusActivity.PublishMessageAsync(activityDto, "activityHistoryQueue", "ACTIVITY_CREATED");

                return request.SupportId;
            }
            catch (UserNotFoundException ex)
            {
               
                throw;
            }
            catch (Exception ex)
            {
               
                throw new ApplicationException("Ocurrió un error inesperado al eliminar el trabajador de soporte.", ex);
            }
        }
    }
}
