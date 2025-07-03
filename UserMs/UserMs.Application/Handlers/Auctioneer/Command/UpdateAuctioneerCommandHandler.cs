using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Application.Commands.Auctioneer;
using UserMs.Commoon.Dtos.Users.Request.Auctioneer;
using UserMs.Core.RabbitMQ;
using UserMs.Core.Repositories.Auctioneer;
using UserMs.Domain.Entities.Auctioneer.ValueObjects;
using UserMs.Domain.Entities.Bidder.ValueObjects;
using UserMs.Domain.Entities;
using UserMs.Domain.Entities.Auctioneer;
using UserMs.Infrastructure.Exceptions;
using UserMs.Commoon.Dtos.Users.Request.User;
using UserMs.Core;
using UserMs.Domain.Entities.UserEntity;
using UserMs.Commoon.Dtos;
using UserMs.Domain.Entities.IUser.ValueObjects;
using UserMs.Core.Repositories.UserRepo;
using UserMs.Infrastructure.Repositories;
using UserMs.Commoon.Dtos.Users.Response.User;
using AutoMapper;
using System.ComponentModel;
using System.Data.Entity.Infrastructure;
using FluentValidation;
using UserMs.Commoon.Dtos.Users.Response.Auctioneer;
using UserMs.Core.Repositories.ActivityHistoryRepo;
using UserMs.Commoon.Dtos.Users.Response.ActivityHistory;
using UserMs.Core.Service.Keycloak;
using UserMs.Application.Validators;
using Microsoft.Extensions.Logging;
using UserMs.Domain.Entities.ActivityHistory;

namespace Handlers.Auctioneer.Command
{
    public class UpdateAuctioneerCommandHandler : IRequestHandler<UpdateAuctioneerCommand, GetAuctioneerDto>
    {
        private readonly IAuctioneerRepository _auctioneerRepository;
        private readonly IAuctioneerRepositoryMongo _auctioneerRepositoryMongo;
        private readonly IEventBus<GetAuctioneerDto> _eventBus;
        private readonly IKeycloakService _keycloakRepository;
        private readonly IEventBus<GetUsersDto> _eventBusUser;
        private readonly IUserRepository _usersRepository;
        private readonly IMapper _mapper;
        private readonly IActivityHistoryRepository _activityHistoryRepository;
        private readonly IUserRepositoryMongo _usersRepositoryMongo;
        private readonly IEventBus<GetActivityHistoryDto> _eventBusActivity;

        public UpdateAuctioneerCommandHandler(
            IEventBus<GetActivityHistoryDto> eventBusActivity,
            IActivityHistoryRepository activityHistoryRepository,
            IAuctioneerRepository auctioneerRepository,
            IAuctioneerRepositoryMongo auctioneerRepositoryMongo,
            IEventBus<GetAuctioneerDto> eventBus,
            IKeycloakService keycloakRepository,
            IMapper mapper,
            IEventBus<GetUsersDto> eventBusUser,
            IUserRepository usersRepository,
            IUserRepositoryMongo usersRepositoryMongo)
        {
            _auctioneerRepository = auctioneerRepository;
            _auctioneerRepositoryMongo = auctioneerRepositoryMongo;
            _eventBus = eventBus;
            _keycloakRepository = keycloakRepository;
            _eventBusUser = eventBusUser;
            _usersRepository = usersRepository;
            _usersRepositoryMongo = usersRepositoryMongo;
            _mapper = mapper;
            _activityHistoryRepository = activityHistoryRepository;
            _eventBusActivity = eventBusActivity;
        }


        public async Task<GetAuctioneerDto?> Handle(UpdateAuctioneerCommand request, CancellationToken cancellationToken)
        {
            try
            {
                await ValidateAuctioneerRequest(request, cancellationToken);

                var existingAuctioneer = await GetExistingAuctioneer(request.AuctioneerId);
                var existingUsers = await GetExistingUser(request.AuctioneerId.Value);

                UpdateAuctioneerEntity(existingAuctioneer, request.Auctioneer, existingUsers);
                var updatedUser = CreateUpdatedUserEntity(request.Auctioneer, existingUsers);

                await SaveUpdatedEntities(existingAuctioneer, updatedUser);
                await PublishUpdateEvents(existingAuctioneer, updatedUser, request.AuctioneerId.Value);
                var aucDto = _mapper.Map<GetAuctioneerDto>(existingAuctioneer);
                return aucDto;
            }
            catch (Exception ex)
            {
                ExceptionHandlerService.HandleException(ex);
                throw;
            }
        }

        private async Task ValidateAuctioneerRequest(UpdateAuctioneerCommand request, CancellationToken cancellationToken)
        {
            var validator = new UpdateAuctioneersValidator();
            var validationResult = await validator.ValidateAsync(request.Auctioneer, cancellationToken);

            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);
        }

        private async Task<Auctioneers?> GetExistingAuctioneer(UserId auctioneerId)
        {
            var existingAuctioneer = await _auctioneerRepositoryMongo.GetAuctioneerByIdAsync(auctioneerId);
            if (existingAuctioneer == null)
                throw new UserNotFoundException("Auctioneer not found.");

            return existingAuctioneer;
        }

        private async Task<Users?> GetExistingUser(Guid userId)
        {
            return await _usersRepositoryMongo.GetUsersById(userId);
        }

        private void UpdateAuctioneerEntity(Auctioneers existingAuctioneer, UpdateAuctioneerDto auctioneerDto, Users existingUsers)
        {
            existingAuctioneer.SetUserEmail(UserEmail.Create(auctioneerDto.UserEmail!));
            existingAuctioneer.SetUserPassword(UserPassword.Create(existingUsers.UserPassword.Value!));
            existingAuctioneer.SetUserLastName(UserLastName.Create(auctioneerDto.UserLastName!));
            existingAuctioneer.SetUserName(UserName.Create(auctioneerDto.UserName!));
            existingAuctioneer.SetUserAddress(UserAddress.Create(auctioneerDto.UserAddress!));
            existingAuctioneer.SetUserPhone(UserPhone.Create(auctioneerDto.UserPhone)!);
            existingAuctioneer.SetAuctioneerDni(AuctioneerDni.Create(auctioneerDto.AuctioneerDni));
            existingAuctioneer.SetAuctioneerBirthday(AuctioneerBirthday.Create(auctioneerDto.AuctioneerBirthday));
            existingAuctioneer.SetAuctioneerDelete(AuctioneerDelete.Create(auctioneerDto.AuctioneerDelete));
        }

        private Users CreateUpdatedUserEntity(UpdateAuctioneerDto auctioneerDto, Users existingUsers)
        {
            return new Users(
                UserId.Create(existingUsers.UserId.Value),
                UserEmail.Create(auctioneerDto.UserEmail),
                UserPassword.Create(existingUsers.UserPassword.Value),
                UserName.Create(auctioneerDto.UserName),
                UserPhone.Create(auctioneerDto.UserPhone),
                UserAddress.Create(auctioneerDto.UserAddress),
                UserLastName.Create(auctioneerDto.UserLastName),
                Enum.Parse<UsersType>("Subastador"), // 🔹 UserType configurado correctamente
                Enum.Parse<UserAvailable>("Activo"),
                UserDelete.Create(false)
            );
        }

        private async Task SaveUpdatedEntities(Auctioneers updatedAuctioneer, Users updatedUser)
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
            _keycloakRepository.UpdateUser(updatedAuctioneer.UserId, updateUserDto);

            await _auctioneerRepository.UpdateAsync(updatedAuctioneer.UserId, updatedAuctioneer);
            await _usersRepository.UpdateUsersAsync(updatedUser.UserId, updatedUser);
        }

        private async Task PublishUpdateEvents(Auctioneers updatedAuctioneer, Users updatedUser, Guid auctioneerId)
        {
            var aucDto = _mapper.Map<GetAuctioneerDto>(updatedAuctioneer);
            await _eventBus.PublishMessageAsync(aucDto, "auctioneerQueue", "AUCTIONEER_UPDATED");

            var usersDto = _mapper.Map<GetUsersDto>(updatedUser);
            await _eventBusUser.PublishMessageAsync(usersDto, "userQueue", "USER_UPDATED");

            var activity = new ActivityHistory(
                Guid.NewGuid(),
                auctioneerId,
                "Actualizó Perfil de Subastador",
                DateTime.UtcNow
            );
            await _activityHistoryRepository.AddAsync(activity);

            var activityDto = _mapper.Map<GetActivityHistoryDto>(activity);
            await _eventBusActivity.PublishMessageAsync(activityDto, "activityHistoryQueue", "ACTIVITY_CREATED");
        }
    }
}