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
using FluentValidation;
using UserMs.Commoon.Dtos.Users.Response.Auctioneer;
using UserMs.Core.Repositories.ActivityHistoryRepo;
using UserMs.Commoon.Dtos.Users.Response.ActivityHistory;
using UserMs.Core.Service.Keycloak;
using UserMs.Application.Validators;
using Microsoft.Extensions.Logging;

namespace UserMs.Application.Handlers.Auctioneer.Command
{
    public class UpdateAuctioneerCommandHandler : IRequestHandler<UpdateAuctioneerCommand, Auctioneers>
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


        public async Task<Auctioneers?> Handle(UpdateAuctioneerCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var existingAuctioneer = await _auctioneerRepositoryMongo.GetAuctioneerByIdAsync(UserId.Create(request.AuctioneerId));
                var existingUsers = await _usersRepositoryMongo.GetUsersById(request.AuctioneerId.Value);

                if (existingAuctioneer == null)
                    throw new UserNotFoundException("Auctioneer not found.");

                var validator = new UpdateAuctioneersValidator();
                var validationResult = await validator.ValidateAsync(request.Auctioneer, cancellationToken);

                if (!validationResult.IsValid)
                    throw new ValidationException(validationResult.Errors);

                // Actualización de propiedades
                existingAuctioneer.SetUserEmail(UserEmail.Create(request.Auctioneer.UserEmail!));
                existingAuctioneer.SetUserPassword(UserPassword.Create(existingUsers.UserPassword.Value!));
                existingAuctioneer.SetUserLastName(UserLastName.Create(request.Auctioneer.UserLastName!));
                existingAuctioneer.SetUserName(UserName.Create(request.Auctioneer.UserName!));
                existingAuctioneer.SetUserAddress(UserAddress.Create(request.Auctioneer.UserAddress!));
                existingAuctioneer.SetUserPhone(UserPhone.Create(request.Auctioneer.UserPhone)!);
                existingAuctioneer.SetAuctioneerDni(AuctioneerDni.Create(request.Auctioneer.AuctioneerDni));
                existingAuctioneer.SetAuctioneerBirthday(AuctioneerBirthday.Create(request.Auctioneer.AuctioneerBirthday));
                existingAuctioneer.SetAuctioneerDelete(AuctioneerDelete.Create(request.Auctioneer.AuctioneerDelete));

                var users = new Users(
                    UserId.Create(request.AuctioneerId.Value),
                    UserEmail.Create(request.Auctioneer.UserEmail),
                    UserPassword.Create(existingUsers.UserPassword.Value),
                    UserName.Create(request.Auctioneer.UserName),
                    UserPhone.Create(request.Auctioneer.UserPhone),
                    UserAddress.Create(request.Auctioneer.UserAddress),
                    UserLastName.Create(request.Auctioneer.UserLastName)
                );

                var updateU = _mapper.Map<UpdateUserDto>(users);

                var aucDto = _mapper.Map<GetAuctioneerDto>(existingAuctioneer);
                _keycloakRepository.UpdateUser(existingAuctioneer.UserId, updateU);
                await _auctioneerRepository.UpdateAsync(existingAuctioneer.UserId, existingAuctioneer);
                await _eventBus.PublishMessageAsync(aucDto, "auctioneerQueue", "AUCTIONEER_UPDATED");

                await _usersRepository.UpdateUsersAsync(existingUsers.UserId, users);
                var usersDto = _mapper.Map<GetUsersDto>(users);
                await _eventBusUser.PublishMessageAsync(usersDto, "userQueue", "USER_UPDATED");

                var activity = new Domain.Entities.ActivityHistory.ActivityHistory(
                    Guid.NewGuid(),
                    usersDto.UserId,
                    "Actualizó Perfil de Subastador",
                    DateTime.UtcNow
                );

                await _activityHistoryRepository.AddAsync(activity);
                var activityDto = _mapper.Map<GetActivityHistoryDto>(activity);
                await _eventBusActivity.PublishMessageAsync(activityDto, "activityHistoryQueue", "ACTIVITY_CREATED");

                return existingAuctioneer;
            }
            catch (ValidationException ex)
            {
                // Manejo de errores de validación específicos
              
                throw;
            }
            catch (UserNotFoundException ex)
            {
               
                throw;
            }
            catch (Exception ex)
            {
               
                throw new ApplicationException("Ocurrió un error al actualizar el subastador.", ex);
            }
        }
    }
}