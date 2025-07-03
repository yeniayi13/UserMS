using AutoMapper;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using UserMs.Application.Commands.Support;
using UserMs.Application.Validators;
using UserMs.Commoon.Dtos;
using UserMs.Commoon.Dtos.Users.Request.Support;
using UserMs.Commoon.Dtos.Users.Request.User;
using UserMs.Commoon.Dtos.Users.Response.ActivityHistory;
using UserMs.Commoon.Dtos.Users.Response.Support;
using UserMs.Commoon.Dtos.Users.Response.User;
using UserMs.Core;
using UserMs.Core.RabbitMQ;
using UserMs.Core.Repositories.ActivityHistoryRepo;
using UserMs.Core.Repositories.Supports;
using UserMs.Core.Repositories.SupportsRepo;
using UserMs.Core.Repositories.UserRepo;
using UserMs.Core.Service.Keycloak;
using UserMs.Domain.Entities;
using UserMs.Domain.Entities.Auctioneer.ValueObjects;
using UserMs.Domain.Entities.IUser.ValueObjects;
using UserMs.Domain.Entities.Support;
using UserMs.Domain.Entities.Support.ValueObjects;
using UserMs.Domain.Entities.UserEntity;
using UserMs.Infrastructure.Exceptions;
using UserMs.Infrastructure.Repositories;

namespace Handlers.Support.Command
{
    public class UpdateSupportCommandHandler : IRequestHandler<UpdateSupportCommand, GetSupportDto>
    {
        private readonly ISupportRepository _supportRepository;
        private readonly IEventBus<GetSupportDto> _eventBus;
        private readonly IKeycloakService _keycloakRepository;
        private readonly IUserRepository _usersRepository;
        private readonly IEventBus<GetUsersDto> _eventBusUser;
        private readonly IMapper _mapper;
        private readonly IActivityHistoryRepository _activityHistoryRepository;
        private readonly IEventBus<GetActivityHistoryDto> _eventBusActivity;
        private readonly ISupportRepositoryMongo _supportRepositoryMongo;
        private readonly IUserRepositoryMongo _usersRepositoryMongo;
        public UpdateSupportCommandHandler(
            ISupportRepository supportRepository,
            ISupportRepositoryMongo supportRepositoryMongo,
            IEventBus<GetSupportDto> eventBus,
            IEventBus<GetUsersDto> eventBusUser,
            IKeycloakService keycloakRepository,
            IUserRepository usersRepository,
            IUserRepositoryMongo usersRepositoryMongo,
            IMapper mapper,
            IEventBus<GetActivityHistoryDto> eventBusActivity,
            IActivityHistoryRepository activityHistoryRepository)
        {
            _supportRepository = supportRepository;
            _supportRepositoryMongo = supportRepositoryMongo;
            _eventBus = eventBus;
            _eventBusUser = eventBusUser;
            _keycloakRepository = keycloakRepository;
            _usersRepository = usersRepository;
            _usersRepositoryMongo = usersRepositoryMongo;
            _mapper = mapper;
            _activityHistoryRepository = activityHistoryRepository;
            _eventBusActivity = eventBusActivity;
        }


        public async Task<GetSupportDto?> Handle(UpdateSupportCommand request, CancellationToken cancellationToken)
        {
            try
            {
                await ValidateSupportRequest(request, cancellationToken);

                var existingSupport = await GetExistingSupport(request.SupportId);
                var existingUsers = await GetExistingUser(request.SupportId.Value);
                var supportSpecialization = ValidateSupportSpecialization(request.Support.SupportSpecialization);

                UpdateSupportEntity(existingSupport, request.Support, existingUsers, supportSpecialization);
                var updatedUser = CreateUpdatedUserEntity(request.Support, existingUsers);

                await SaveUpdatedEntities(existingSupport, updatedUser);
                await PublishUpdateEvents(existingSupport, updatedUser, request.SupportId.Value);
                var supportDto = _mapper.Map<GetSupportDto>(existingSupport);
                return supportDto;
            }
            catch (Exception ex)
            {
                ExceptionHandlerService.HandleException(ex);
                throw;
            }
        }

        private async Task ValidateSupportRequest(UpdateSupportCommand request, CancellationToken cancellationToken)
        {
            var validator = new UpdateSupportValidator();
            var validationResult = await validator.ValidateAsync(request.Support, cancellationToken);

            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);
        }

        private async Task<Supports?> GetExistingSupport(UserId supportId)
        {
            var existingSupport = await _supportRepositoryMongo.GetSupportByIdAsync(supportId);
            if (existingSupport == null)
                throw new UserNotFoundException("Support not found.");

            return existingSupport;
        }

        private async Task<Users?> GetExistingUser(Guid userId)
        {
            return await _usersRepositoryMongo.GetUsersById(userId);
        }
        private SupportSpecialization ValidateSupportSpecialization(string specialization)
        {
            if (!Enum.TryParse<SupportSpecialization>(specialization, out var supportSpecialization))
                throw new ArgumentException("Invalid SupportSpecialization");

            return supportSpecialization;
        }

        private void UpdateSupportEntity(Supports existingSupport, UpdateSupportDto supportDto, Users existingUsers, SupportSpecialization specialization)
        {
            existingSupport.SetSupportDni(SupportDni.Create(supportDto.SupportDni));
            existingSupport.SetSupportSpecialization(specialization);
            existingSupport.SetUserEmail(UserEmail.Create(supportDto.UserEmail));
            existingSupport.SetUserPassword(UserPassword.Create(existingUsers.UserPassword.Value));
            existingSupport.SetUserName(UserName.Create(supportDto.UserName));
            existingSupport.SetUserLastName(UserLastName.Create(supportDto.UserLastName));
            existingSupport.SetUserPhone(UserPhone.Create(supportDto.UserPhone));
            existingSupport.SetUserAddress(UserAddress.Create(supportDto.UserAddress));
            existingSupport.SetSupportDelete(SupportDelete.Create(supportDto.SupportDelete));
        }

        private Users CreateUpdatedUserEntity(UpdateSupportDto supportDto, Users existingUsers)
        {
            return new Users(
                UserId.Create(existingUsers.UserId.Value),
                UserEmail.Create(supportDto.UserEmail),
                UserPassword.Create(existingUsers.UserPassword.Value),
                UserName.Create(supportDto.UserName),
                UserPhone.Create(supportDto.UserPhone),
                UserAddress.Create(supportDto.UserAddress),
                UserLastName.Create(supportDto.UserLastName)
            );
        }

        private async Task SaveUpdatedEntities(Supports updatedSupport, Users updatedUser)
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
            _keycloakRepository.UpdateUser(updatedSupport.UserId, updateUserDto);

            await _supportRepository.UpdateAsync(updatedSupport.UserId, updatedSupport);
            await _usersRepository.UpdateUsersAsync(updatedUser.UserId, updatedUser);
        }

        private async Task PublishUpdateEvents(Supports updatedSupport, Users updatedUser, Guid supportId)
        {
            var supportDto = _mapper.Map<GetSupportDto>(updatedSupport);
            await _eventBus.PublishMessageAsync(supportDto, "supportQueue", "SUPPORT_UPDATED");

            var usersDto = _mapper.Map<GetUsersDto>(updatedUser);
            await _eventBusUser.PublishMessageAsync(usersDto, "userQueue", "USER_UPDATED");

            var activity = new UserMs.Domain.Entities.ActivityHistory.ActivityHistory(
                Guid.NewGuid(),
                supportId,
                "Actualizó perfil de un trabajador de soporte",
                DateTime.UtcNow
            );
            await _activityHistoryRepository.AddAsync(activity);

            var activityDto = _mapper.Map<GetActivityHistoryDto>(activity);
            await _eventBusActivity.PublishMessageAsync(activityDto, "activityHistoryQueue", "ACTIVITY_CREATED");
        }
    }
}
