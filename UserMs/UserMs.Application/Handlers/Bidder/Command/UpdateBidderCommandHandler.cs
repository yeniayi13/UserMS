
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
using UserMs.Commoon.Dtos.Users.Response.Auctioneer;

namespace UserMs.Application.Handlers.Bidder.Command
{
    public class UpdateBidderCommandHandler : IRequestHandler<UpdateBidderCommand, GetBidderDto>
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


        public async Task<GetBidderDto?> Handle(UpdateBidderCommand request, CancellationToken cancellationToken)
        {
            try
            {
                await ValidateBidderRequest(request, cancellationToken);

                var existingBidder = await GetExistingBidder(request.BidderId);
                var existingUsers = await GetExistingUser(request.BidderId.Value);

                UpdateBidderEntity(existingBidder, request.Bidder, existingUsers);
                var updatedUser = CreateUpdatedUserEntity(request.Bidder, existingUsers);

                await SaveUpdatedEntities(existingBidder, updatedUser);
                await PublishUpdateEvents(existingBidder, updatedUser, request.BidderId.Value);
                var bidderDto = _mapper.Map<GetBidderDto>(existingBidder);
                return bidderDto;
            }
            catch (Exception ex)
            {
                ExceptionHandlerService.HandleException(ex);
                throw;
            }
        }
        private async Task ValidateBidderRequest(UpdateBidderCommand request, CancellationToken cancellationToken)
        {
            var validator = new UpdateBidderValidator();
            var validationResult = await validator.ValidateAsync(request.Bidder, cancellationToken);

            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);
        }

        private async Task<Bidders?> GetExistingBidder(UserId bidderId)
        {
            var existingBidder = await _bidderRepositoryMongo.GetBidderByIdAsync(bidderId);
            if (existingBidder == null)
                throw new UserNotFoundException("Bidder not found.");

            return existingBidder;
        }

        private async Task<Users?> GetExistingUser(Guid userId)
        {
            return await _usersRepositoryMongo.GetUsersById(userId);
        }

        private void UpdateBidderEntity(Bidders existingBidder, UpdateBidderDto bidderDto, Users existingUsers)
        {
            existingBidder.SetUserEmail(UserEmail.Create(bidderDto.UserEmail!));
            existingBidder.SetUserPassword(UserPassword.Create(existingUsers.UserPassword.Value!));
            existingBidder.SetUserLastName(UserLastName.Create(bidderDto.UserLastName!));
            existingBidder.SetUserName(UserName.Create(bidderDto.UserName!));
            existingBidder.SetUserAddress(UserAddress.Create(bidderDto.UserAddress!));
            existingBidder.SetUserPhone(UserPhone.Create(bidderDto.UserPhone)!);
            existingBidder.SetBidderDni(BidderDni.Create(bidderDto.BidderDni));
            existingBidder.SetBidderBirthday(BidderBirthday.Create(bidderDto.BidderBirthday));
            existingBidder.SetBidderDelete(BidderDelete.Create(bidderDto.BidderDelete));
        }

        private Users CreateUpdatedUserEntity(UpdateBidderDto bidderDto, Users existingUsers)
        {
            return new Users(
                UserId.Create(existingUsers.UserId.Value),
                UserEmail.Create(bidderDto.UserEmail),
                UserPassword.Create(existingUsers.UserPassword.Value),
                UserName.Create(bidderDto.UserName),
                UserPhone.Create(bidderDto.UserPhone),
                UserAddress.Create(bidderDto.UserAddress),
                UserLastName.Create(bidderDto.UserLastName),
                Enum.Parse<UsersType>("Postor"), // 🔹 UserType configurado correctamente
                Enum.Parse<UserAvailable>("Activo"),
                UserDelete.Create(false)
            );
        }

        private async Task SaveUpdatedEntities(Bidders updatedBidder, Users updatedUser)
        {
            var updateUserDto = new UpdateUserDto
            {

                UserEmail = updatedUser.UserEmail.Value,
                UserPassword = updatedUser.UserPassword.Value,
                UserName = updatedUser.UserName.Value,
                UserLastName = updatedUser.UserLastName.Value,
                UserPhone = updatedUser.UserPhone.Value,
                UserAddress = updatedUser.UserAddress.Value,

            };
            _keycloakRepository.UpdateUser(updatedBidder.UserId, updateUserDto);

            await _bidderRepository.UpdateAsync(updatedBidder.UserId, updatedBidder);
            await _usersRepository.UpdateUsersAsync(updatedUser.UserId, updatedUser);
        }

        private async Task PublishUpdateEvents(Bidders updatedBidder, Users updatedUser, Guid bidderId)
        {
            var bidderDto = _mapper.Map<GetBidderDto>(updatedBidder);
            await _eventBus.PublishMessageAsync(bidderDto, "bidderQueue", "BIDDER_UPDATED");

            var userDto = _mapper.Map<GetUsersDto>(updatedUser);
            await _eventBusUser.PublishMessageAsync(userDto, "userQueue", "USER_UPDATED");

            var activity = new Domain.Entities.ActivityHistory.ActivityHistory(
                Guid.NewGuid(),
                bidderId,
                "Actualizó perfil de Postor",
                DateTime.UtcNow
            );
            await _activityHistoryRepository.AddAsync(activity);

            var activityDto = _mapper.Map<GetActivityHistoryDto>(activity);
            await _eventBusActivity.PublishMessageAsync(activityDto, "activityHistoryQueue", "ACTIVITY_CREATED");
        }
    }
}
