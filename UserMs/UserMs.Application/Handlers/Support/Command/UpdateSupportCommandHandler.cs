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

namespace UserMs.Application.Handlers.Support.Command
{
    public class UpdateSupportCommandHandler : IRequestHandler<UpdateSupportCommand, Supports>
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


        public async Task<Supports?> Handle(UpdateSupportCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var existingSupport = await _supportRepositoryMongo.GetSupportByIdAsync(UserId.Create(request.SupportId));
                var existingUsers = await _usersRepositoryMongo.GetUsersById(request.SupportId.Value);

                // Validación del ENUM SupportSpecialization
                if (!Enum.TryParse<SupportSpecialization>(request.Support.SupportSpecialization,
                        out var supportSpecialization))
                {
                    throw new ArgumentException("Invalid SupportSpecialization");
                }


                if (existingSupport == null)
                    throw new UserNotFoundException("Auctioneer not found.");

                var validator = new UpdateSupportValidator();
                var validationResult = await validator.ValidateAsync(request.Support, cancellationToken);

                if (!validationResult.IsValid)
                    throw new ValidationException(validationResult.Errors);

                // Actualización de datos del Support
                existingSupport.SetSupportDni(SupportDni.Create(request.Support.SupportDni));
                existingSupport.SetSupportSpecialization(supportSpecialization);
                existingSupport.SetUserEmail(UserEmail.Create(request.Support.UserEmail));
                existingSupport.SetUserPassword(UserPassword.Create(request.Support.UserPassword));
                existingSupport.SetUserName(UserName.Create(request.Support.UserName));
                existingSupport.SetUserLastName(UserLastName.Create(request.Support.UserLastName));
                existingSupport.SetUserPhone(UserPhone.Create(request.Support.UserPhone));
                existingSupport.SetUserAddress(UserAddress.Create(request.Support.UserAddress));
                existingSupport.SetSupportDelete(SupportDelete.Create(request.Support.SupportDelete));

                var users = new Users(
                    UserId.Create(request.SupportId.Value),
                    UserEmail.Create(request.Support.UserEmail),
                    UserPassword.Create(request.Support.UserPassword),
                    UserName.Create(request.Support.UserName),
                    UserPhone.Create(request.Support.UserPhone),
                    UserAddress.Create(request.Support.UserAddress),
                    UserLastName.Create(request.Support.UserLastName)
                );

                var updateU = _mapper.Map<UpdateUserDto>(users);

                var supportDto = _mapper.Map<GetSupportDto>(existingSupport);

                // Actualización en Keycloak y bases de datos
                _keycloakRepository.UpdateUser(existingSupport.UserId, updateU);
                await _supportRepository.UpdateAsync(existingSupport.UserId, existingSupport);
                await _eventBus.PublishMessageAsync(supportDto, "supportQueue", "SUPPORT_UPDATED");

                await _usersRepository.UpdateUsersAsync(existingUsers.UserId, users);
                var usersDto = _mapper.Map<GetUsersDto>(users);
                await _eventBusUser.PublishMessageAsync(usersDto, "userQueue", "USER_UPDATED");

                var activity = new Domain.Entities.ActivityHistory.ActivityHistory(
                    Guid.NewGuid(),
                    usersDto.UserId,
                    "Actualizó perfil de un trabajador de soporte",
                    DateTime.UtcNow
                );
                await _activityHistoryRepository.AddAsync(activity);
                var activityDto = _mapper.Map<GetActivityHistoryDto>(activity);
                await _eventBusActivity.PublishMessageAsync(activityDto, "activityHistoryQueue", "ACTIVITY_CREATED");

                return existingSupport;
            }
            catch (ArgumentException ex)
            {

                throw;
            }
            catch (NullAtributeException ex)
            {

                throw;
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

                throw new ApplicationException("Ocurrió un error inesperado al actualizar el trabajador de soporte.",
                    ex);
            }
        }
    }
}
