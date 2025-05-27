using AutoMapper;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Application.Commands.Auctioneer;
using UserMs.Commoon.Dtos.Users.Response.Auctioneer;
using UserMs.Core.RabbitMQ;
using UserMs.Core.Repositories.Auctioneer;
using UserMs.Domain.Entities.Auctioneer.ValueObjects;
using UserMs.Domain.Entities;
using UserMs.Commoon.Dtos.Users.Response.User;
using UserMs.Core.Repositories.UserRepo;
using UserMs.Core;
using UserMs.Infrastructure.Repositories;
using UserMs.Core.Repositories.ActivityHistoryRepo;
using UserMs.Commoon.Dtos.Users.Response.ActivityHistory;
using UserMs.Core.Service.Keycloak;
using UserMs.Infrastructure.Exceptions;

namespace UserMs.Application.Handlers.Auctioneer.Command
{
    public class DeleteAuctioneerCommandHandler : IRequestHandler<DeleteAuctioneerCommand, UserId>
    {
        private readonly IAuctioneerRepository _auctioneerRepository;
        private readonly IAuctioneerRepositoryMongo _auctioneerRepositoryMongo;
        private readonly IEventBus<GetAuctioneerDto> _eventBus;
        private readonly IMapper _mapper;
        private readonly IKeycloakService _keycloakMsService;
        private readonly IEventBus<GetUsersDto> _eventBusUser;
        private readonly IUserRepository _usersRepository;
        private readonly IUserRepositoryMongo _usersRepositoryMongo;
        private readonly IActivityHistoryRepository _activityHistoryRepository;
        private readonly IEventBus<GetActivityHistoryDto> _eventBusActivity;

        public DeleteAuctioneerCommandHandler(
            IEventBus<GetActivityHistoryDto> eventBusActivity,
            IActivityHistoryRepository activityHistoryRepository,
            IAuctioneerRepository auctioneerRepository,
            IAuctioneerRepositoryMongo auctioneerRepositoryMongo,
            IUserRepository usersRepository,
            IUserRepositoryMongo usersRepositoryMongo,
            IKeycloakService keycloakMsService,
            IEventBus<GetUsersDto> eventBusUser,
            IEventBus<GetAuctioneerDto> eventBus,
            IMapper mapper)
        {
            _auctioneerRepository = auctioneerRepository;
            _auctioneerRepositoryMongo = auctioneerRepositoryMongo;
            _eventBus = eventBus;
            _mapper = mapper;
            _keycloakMsService = keycloakMsService;
            _eventBusUser = eventBusUser;
            _usersRepository = usersRepository;
            _usersRepositoryMongo = usersRepositoryMongo;
            _activityHistoryRepository = activityHistoryRepository;
            _eventBusActivity = eventBusActivity;
        }


        public async Task<UserId> Handle(DeleteAuctioneerCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var auctioneer =
                    await _auctioneerRepositoryMongo.GetAuctioneerByIdAsync(UserId.Create(request.AuctioneerId));
                var users = await _usersRepositoryMongo.GetUsersById(request.AuctioneerId);

                if (auctioneer == null)
                    throw new UserNotFoundException("Auctioneer not found.");

                // Eliminación del subastador
                await _auctioneerRepository.DeleteAsync(request.AuctioneerId);
                await _keycloakMsService.DeleteUserAsync(auctioneer.UserId);
                await _usersRepository.DeleteUsersAsync(request.AuctioneerId);

                // Publicar eventos en el bus de mensajes
                var auctioneerDto = _mapper.Map<GetAuctioneerDto>(auctioneer);
                await _eventBus.PublishMessageAsync(auctioneerDto, "auctioneerQueue", "AUCTIONEER_DELETED");

                var usersDto = _mapper.Map<GetUsersDto>(users);
                await _eventBusUser.PublishMessageAsync(usersDto, "userQueue", "USER_DELETED");

                // Registrar actividad
                var activity = new Domain.Entities.ActivityHistory.ActivityHistory(
                    Guid.NewGuid(),
                    auctioneer.UserId.Value,
                    "Eliminó Subastador",
                    DateTime.UtcNow
                );
                await _activityHistoryRepository.AddAsync(activity);
                var activityDto = _mapper.Map<GetActivityHistoryDto>(activity);
                await _eventBusActivity.PublishMessageAsync(activityDto, "activityHistoryQueue", "ACTIVITY_CREATED");

                return request.AuctioneerId;
            }
            catch (Exception ex)
            {
                ExceptionHandlerService.HandleException(ex);
                throw;
            }
        }
    }
}
