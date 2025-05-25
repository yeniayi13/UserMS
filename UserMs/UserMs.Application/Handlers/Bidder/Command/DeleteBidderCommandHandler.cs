using AutoMapper;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Application.Commands.Auctioneer;
using UserMs.Application.Commands.Bidder;
using UserMs.Commoon.Dtos.Users.Response.ActivityHistory;
using UserMs.Commoon.Dtos.Users.Response.Auctioneer;
using UserMs.Commoon.Dtos.Users.Response.Bidder;
using UserMs.Commoon.Dtos.Users.Response.User;
using UserMs.Core;
using UserMs.Core.RabbitMQ;
using UserMs.Core.Repositories.ActivityHistoryRepo;
using UserMs.Core.Repositories.Auctioneer;
using UserMs.Core.Repositories.Biddere;
using UserMs.Core.Repositories.Bidders;
using UserMs.Core.Repositories.UserRepo;
using UserMs.Core.Service.Keycloak;
using UserMs.Domain.Entities;
using UserMs.Domain.Entities.Auctioneer.ValueObjects;
using UserMs.Domain.Entities.Bidder.ValueObjects;
using UserMs.Domain.Entities.UserEntity;
using UserMs.Infrastructure.Exceptions;

namespace UserMs.Application.Handlers.Bidder.Command
{
    public class DeleteBidderCommandHandler : IRequestHandler<DeleteBidderCommand, UserId>
    {
        private readonly IBidderRepository _bidderRepository;
        private readonly IBidderRepositoryMongo _bidderRepositoryMongo;
        private readonly IEventBus<GetBidderDto> _eventBus;
        private readonly IMapper _mapper;
        private readonly IKeycloakService _keycloakMsService;
        private readonly IEventBus<GetUsersDto> _eventBusUser;
        private readonly IUserRepository _usersRepository;
        private readonly IUserRepositoryMongo _usersRepositoryMongo;
        private readonly IActivityHistoryRepository _activityHistoryRepository;
        private readonly IEventBus<GetActivityHistoryDto> _eventBusActivity;
        public DeleteBidderCommandHandler(
            IEventBus<GetActivityHistoryDto> eventBusActivity,
            IActivityHistoryRepository activityHistoryRepository,
            IBidderRepository bidderRepository,
            IBidderRepositoryMongo bidderRepositoryMongo,
            IUserRepository usersRepository,
            IUserRepositoryMongo usersRepositoryMongo,
            IKeycloakService keycloakMsService,
            IEventBus<GetUsersDto> eventBusUser,
            IEventBus<GetBidderDto> eventBus,
            IMapper mapper)
        {
            _bidderRepository = bidderRepository;
            _bidderRepositoryMongo = bidderRepositoryMongo;
            _eventBus = eventBus;
            _mapper = mapper;
            _keycloakMsService = keycloakMsService;
            _eventBusUser = eventBusUser;
            _usersRepository = usersRepository;
            _usersRepositoryMongo = usersRepositoryMongo;
            _activityHistoryRepository = activityHistoryRepository;
            _eventBusActivity = eventBusActivity;
        }


        public async Task<UserId> Handle(DeleteBidderCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var bidder = await _bidderRepositoryMongo.GetBidderByIdAsync(request.BidderId);
                var users = await _usersRepositoryMongo.GetUsersById(request.BidderId);

                if (bidder == null)
                    throw new UserNotFoundException("Bidder not found.");

                bidder.SetBidderDelete(BidderDelete.Create(true));

                await _bidderRepository.DeleteAsync(request.BidderId);
                await _keycloakMsService.DeleteUserAsync(bidder!.UserId);
                await _usersRepository.DeleteUsersAsync(request.BidderId);

                var bidderDto = _mapper.Map<GetBidderDto>(bidder);
                await _eventBus.PublishMessageAsync(bidderDto, "bidderQueue", "BIDDER_DELETED");

                var usersDto = _mapper.Map<GetUsersDto>(users);
                await _eventBusUser.PublishMessageAsync(usersDto, "userQueue", "USER_DELETED");

                var activity = new Domain.Entities.ActivityHistory.ActivityHistory(
                    Guid.NewGuid(),
                    request.BidderId.Value,
                    "Eliminó Postor",
                    DateTime.UtcNow
                );
                await _activityHistoryRepository.AddAsync(activity);
                var activityDto = _mapper.Map<GetActivityHistoryDto>(activity);
                await _eventBusActivity.PublishMessageAsync(activityDto, "activityHistoryQueue", "ACTIVITY_CREATED");

                return request.BidderId;
            }
            catch (UserNotFoundException ex)
            {
               
                throw;
            }
          
            catch (Exception ex)
            {
                
                throw new ApplicationException("Ocurrió un error inesperado al eliminar el postor.", ex);
            }
        }
    }
}
