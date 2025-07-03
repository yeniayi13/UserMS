using AuthMs.Common.Exceptions;
using AutoMapper;
using ClaimsMS.Core.Service.Notification;
using FluentValidation;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Application.Commands.Users;
using UserMs.Application.Validators;
using UserMs.Common.Dtos.Response;
using UserMs.Commoon.Dtos.Users.Response.ActivityHistory;
using UserMs.Commoon.Dtos.Users.Response.User;
using UserMs.Core.RabbitMQ;
using UserMs.Core.Repositories.ActivityHistoryRepo;
using UserMs.Core.Repositories.UserRepo;
using UserMs.Core.Service.Keycloak;
using UserMs.Domain.Entities;
using UserMs.Domain.Entities.ActivityHistory;
using UserMs.Domain.Entities.UserEntity;
using UserMs.Infrastructure.Exceptions;

namespace Application.Handlers.User.Commads
{
    public class UpdatePasswordCommandHandler : IRequestHandler<UpdatePasswordCommand, Guid>
    {
        private readonly IKeycloakService _keycloakService;
        private readonly IActivityHistoryRepository _activityHistoryRepository;
        private readonly IEventBus<GetActivityHistoryDto> _eventBusActivity;
        private readonly IEventBus<GetUsersDto> _eventBusUser;
        private readonly IMapper _mapper;
        private readonly IUserRepository _userRepository;
        private readonly IUserRepositoryMongo _userRepositoryMongo;
        private readonly INotificationService _notificationService;
        public UpdatePasswordCommandHandler(
        IKeycloakService keycloakService,
        IActivityHistoryRepository activityHistoryRepository,
        IEventBus<GetActivityHistoryDto> eventBusActivity, IMapper mapper, IUserRepository userRepository
            , IUserRepositoryMongo userRepositoryMongo, IEventBus<GetUsersDto> eventBusUser, INotificationService notificationService)

        {
            _keycloakService = keycloakService;
            _activityHistoryRepository = activityHistoryRepository;
            _eventBusActivity = eventBusActivity;
            _mapper = mapper;
            _userRepository = userRepository;
            _userRepositoryMongo = userRepositoryMongo;
            _eventBusUser = eventBusUser;
            _notificationService = notificationService;
        }

        public async Task<Guid> Handle(UpdatePasswordCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Validar la entrada del comando UpdatePasswordCommand
                await ValidateInputAsync(request, cancellationToken);

                // Verificar si el usuario existe en la base de datos
                var user = await GetUserAsync(request.UserId);

                // Verificar si la contraseña actual es correcta y si es asi la actualiza en keycloak
                await UpdateKeycloakAsync(user.UserEmail.Value, request.UpdatePasswordDto.Password, request.UpdatePasswordDto.NewPassword);
                // Actualizar la contraseña del usuario en la base de datos
                await UpdatePasswordBdAsync(user, request.UpdatePasswordDto.NewPassword);
                // Publicar el evento de actualización del usuario y notificar al usuario
                await PublishUserUpdateAsync(user);
                // Notificar al usuario sobre la actualización de su contraseña
                await NotifyUserAsync(user.UserId);
                // Registrar la actividad de actualización de contraseña
                await ActivityAsync(request.UserId);

                return user.UserId.Value;
            }
            catch (ValidationException ex)
            {
                
                throw;
            }
            catch (UserNotFoundException ex)
            {
               
                throw;
            }
            catch (KeycloakException ex)
            {
                
                throw;
            }
            catch (Exception ex)
            {
               
                throw new ApplicationException("Error Al actualizar la contraseña.", ex);
            }
        }

        // Valida la entrada del comando UpdatePasswordCommanda
        private async Task ValidateInputAsync(UpdatePasswordCommand request, CancellationToken cancellationToken)
        {
            var validator = new UpdatePasswordValidator();
            var validationResult = await validator.ValidateAsync(request.UpdatePasswordDto, cancellationToken);
            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);

            var currentPassword = request.UpdatePasswordDto.Password;
            var newPassword = request.UpdatePasswordDto.NewPassword;

            if (currentPassword == newPassword)
            {
                throw new ValidationException("El nuevo password no puede ser igual al actual.");
            }


        }

        // Notifica al usuario sobre la actualización de su contraseña
        private async Task NotifyUserAsync(Guid userId)
        {
            var notification = new GetNotificationDto
            {
                NotificationId = Guid.NewGuid(),
                NotificationUserId = userId,
                NotificationSubject = "Actualización de contraseña",
                NotificationMessage = $"Su contraseña ha sido actualizada exitosamente el {DateTime.UtcNow:dd/MM/yyyy}. Si usted no realizó esta acción, por favor contacte inmediatamente al soporte técnico. De lo contrario, no se requiere ninguna acción adicional.",
                NotificationDateTime = DateTime.UtcNow,
                NotificationStatus = "Enviado"
            };

            await _notificationService.SendNotificationAsync(notification);
        }

        // Obtiene el usuario por su ID
        private async Task<Users> GetUserAsync(Guid userId)
        {
            var user = _userRepositoryMongo.GetUsersById(userId);
            if (user == null)
            {
                throw new UserNotFoundException("El usuario no existe.");

            }
            return user.Result;
        }

        // Actualiza la contraseña del usuario en Keycloak
        private async Task UpdateKeycloakAsync(string email, string oldPwd, string newPwd)
        {
            var success = await _keycloakService.ChangeUserPasswordSecureAsync(email, oldPwd, newPwd);
            if (!success)
                throw new KeycloakException("No se pudo actualizar la contraseña en Keycloak.");
        }

        // Actualiza la contraseña del usuario en la base de datos
        private async Task UpdatePasswordBdAsync(Users user, string newPwd)
        {
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(newPwd);
            user.SetUserPassword(UserPassword.Create(hashedPassword));
            await _userRepository.UpdateUsersAsync(user.UserId, user);
        }

        //Actualiza el usuario y publica el evento de actualización
        private async Task PublishUserUpdateAsync(Users user)
        {
            var dto = _mapper.Map<GetUsersDto>(user);
            await _eventBusUser.PublishMessageAsync(dto, "userQueue", "USER_UPDATED");
        }

        // Registra la actividad de actualización de contraseña en el repositorio y la publica en el bus de eventos
        private async Task ActivityAsync(UserId userId)
        {
            var activity = new ActivityHistory(
                Guid.NewGuid(),
                userId,
                "Actualizo la contraseña desde el perfil ",
                DateTime.UtcNow
            );

            await _activityHistoryRepository.AddAsync(activity);

            var dto = _mapper.Map<GetActivityHistoryDto>(activity);
            await _eventBusActivity.PublishMessageAsync(dto, "activityHistoryQueue", "ACTIVITY_CREATED");
        }

    }
}
