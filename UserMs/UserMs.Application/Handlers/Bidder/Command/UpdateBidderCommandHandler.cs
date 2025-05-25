
using AutoMapper;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using UserMs.Application.Commands.Bidder;
using UserMs.Application.Validators;
using UserMs.Commoon.Dtos;
using UserMs.Commoon.Dtos.Users.Request.Bidder;
using UserMs.Commoon.Dtos.Users.Response.ActivityHistory;
using UserMs.Commoon.Dtos.Users.Response.Bidder;
using UserMs.Commoon.Dtos.Users.Response.User;
using UserMs.Core;
using UserMs.Core.RabbitMQ;
using UserMs.Core.Repositories.ActivityHistoryRepo;
using UserMs.Core.Repositories.Biddere;
using UserMs.Core.Repositories.Bidders;
using UserMs.Core.Repositories.UserRepo;
using UserMs.Core.Service.Keycloak;
using UserMs.Domain.Entities;
using UserMs.Domain.Entities.Auctioneer.ValueObjects;
using UserMs.Domain.Entities.Bidder;
using UserMs.Domain.Entities.Bidder.ValueObjects;
using UserMs.Domain.Entities.IUser.ValueObjects;
using UserMs.Domain.Entities.UserEntity;
using UserMs.Infrastructure.Exceptions;

namespace UserMs.Application.Handlers.Bidder.Command
{
    public class UpdateBidderCommandHandler : IRequestHandler<UpdateBidderCommand, Bidders>
    {
        private readonly IBidderRepository _bidderRepository;
        private readonly IBidderRepositoryMongo _bidderRepositoryMongo;
        private readonly IEventBus<GetBidderDto> _eventBus;
        private readonly IUserRepository _usersRepository;
        private readonly IUserRepositoryMongo _usersRepositoryMongo;
        private readonly IEventBus<GetUsersDto> _eventBusUser;
        private readonly IMapper _mapper;
        private readonly IKeycloakService _keycloakRepository;
        private readonly IActivityHistoryRepository _activityHistoryRepository;
        private readonly IEventBus<GetActivityHistoryDto> _eventBusActivity;

        public UpdateBidderCommandHandler(
            IBidderRepository bidderRepository,
            IBidderRepositoryMongo bidderRepositoryMongo,
            IEventBus<GetBidderDto> eventBus,
            IUserRepository usersRepository,
            IUserRepositoryMongo usersRepositoryMongo,
            IEventBus<GetUsersDto> eventBusUser,
            IMapper mapper,
            IKeycloakService keycloakRepository,
            IActivityHistoryRepository activityHistoryRepository,
            IEventBus<GetActivityHistoryDto> eventBusActivity)
        {
            _bidderRepository = bidderRepository;
            _bidderRepositoryMongo = bidderRepositoryMongo;
            _eventBus = eventBus;
            _usersRepository = usersRepository;
            _usersRepositoryMongo = usersRepositoryMongo;
            _eventBusUser = eventBusUser;
            _mapper = mapper;
            _keycloakRepository = keycloakRepository;
            _activityHistoryRepository = activityHistoryRepository;
            _eventBusActivity = eventBusActivity;
        }


        public async Task<Bidders?> Handle(UpdateBidderCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var existingBidder = await _bidderRepositoryMongo.GetBidderByIdAsync(UserId.Create(request.BidderId));
                var existingUsers = await _usersRepositoryMongo.GetUsersById(request.BidderId.Value);

                if (existingBidder == null)
                    throw new UserNotFoundException("Bidder not found.");

                var validator = new UpdateBidderValidator();
                var validationResult = await validator.ValidateAsync(request.Bidder, cancellationToken);

                if (!validationResult.IsValid)
                    throw new ValidationException(validationResult.Errors);

                // Actualización de propiedades
                existingBidder.SetUserEmail(UserEmail.Create(request.Bidder.UserEmail!));
                existingBidder.SetUserPassword(UserPassword.Create(existingUsers.UserPassword.Value!));
                existingBidder.SetUserLastName(UserLastName.Create(request.Bidder.UserLastName!));
                existingBidder.SetUserName(UserName.Create(request.Bidder.UserName!));
                existingBidder.SetUserAddress(UserAddress.Create(request.Bidder.UserAddress!));
                existingBidder.SetUserPhone(UserPhone.Create(request.Bidder.UserPhone)!);
                existingBidder.SetBidderDni(BidderDni.Create(request.Bidder.BidderDni));
                existingBidder.SetBidderBirthday(BidderBirthday.Create(request.Bidder.BidderBirthday));
                existingBidder.SetBidderDelete(BidderDelete.Create(request.Bidder.BidderDelete));

                var users = new Users(
                    UserId.Create(request.BidderId.Value),
                    UserEmail.Create(request.Bidder.UserEmail),
                    UserPassword.Create(existingUsers.UserPassword.Value),
                    UserName.Create(request.Bidder.UserName),
                    UserPhone.Create(request.Bidder.UserPhone),
                    UserAddress.Create(request.Bidder.UserAddress),
                    UserLastName.Create(request.Bidder.UserLastName)
                );

                var updateU = _mapper.Map<UpdateUserDto>(users);

                var bidderDto = _mapper.Map<GetBidderDto>(existingBidder);
                _keycloakRepository.UpdateUser(existingBidder.UserId, updateU);
                await _bidderRepository.UpdateAsync(existingBidder.UserId, existingBidder);
                await _eventBus.PublishMessageAsync(bidderDto, "bidderQueue", "BIDDER_UPDATED");

                await _usersRepository.UpdateUsersAsync(existingUsers.UserId, users);
                var usersDto = _mapper.Map<GetUsersDto>(users);
                await _eventBusUser.PublishMessageAsync(usersDto, "userQueue", "USER_UPDATED");

                var activity = new Domain.Entities.ActivityHistory.ActivityHistory(
                    Guid.NewGuid(),
                    request.BidderId.Value,
                    "Actualizó perfil de Postor",
                    DateTime.UtcNow
                );

                await _activityHistoryRepository.AddAsync(activity);
                var activityDto = _mapper.Map<GetActivityHistoryDto>(activity);
                await _eventBusActivity.PublishMessageAsync(activityDto, "activityHistoryQueue", "ACTIVITY_CREATED");

                return existingBidder;
            }
            catch (ValidationException ex)
            {
                
                throw;
            }
            catch (UserNotFoundException ex)
            {
                
                throw;
            }
            catch (Exception ex)
            {
               
                throw new ApplicationException("Ocurrió un error al actualizar el postor.", ex);
            }
        }
    }
}
