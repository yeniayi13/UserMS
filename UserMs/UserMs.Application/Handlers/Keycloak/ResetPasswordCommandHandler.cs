using AuthMs.Common.Exceptions;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using UserMs.Application.Commands.Keycloak;
using UserMs.Commoon.Dtos.Users.Response.ActivityHistory;
using UserMs.Core.RabbitMQ;
using UserMs.Core.Repositories.ActivityHistoryRepo;
using UserMs.Core.Service.Keycloak;
using UserMs.Domain.Entities.IUser.ValueObjects;

namespace UserMs.Application.Handlers.Keycloak
{
    public class ResetPasswordCommandHandler: IRequestHandler<ResetPasswordCommand, bool>
    {
        private readonly IKeycloakService _keycloakService;
        private readonly IActivityHistoryRepository _activityHistoryRepository;
        private readonly IEventBus<GetActivityHistoryDto> _eventBusActivity;
        private readonly IMapper _mapper;
    public ResetPasswordCommandHandler(
        IKeycloakService keycloakService,
        IActivityHistoryRepository activityHistoryRepository,
        IEventBus<GetActivityHistoryDto> eventBusActivity, IMapper mapper)

    {
        _keycloakService = keycloakService;
        _activityHistoryRepository = activityHistoryRepository;
        _eventBusActivity = eventBusActivity;
        _mapper = mapper;
    }

        public async Task<bool> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.ResetDto.UserEmail))
            {
                throw new ArgumentException("El correo electrónico no puede estar vacío.");
            }

            try
            {
                // 🔹 Obtener el userId del usuario
                var Id = await _keycloakService.GetUserByUserName(request.ResetDto.UserEmail);

                if (Id == Guid.Empty)
                {
                    throw new KeycloakException("No se encontró ningún usuario con el email proporcionado.");
                }

                // 🔹 Enviar el correo de recuperación de contraseña
                var result = await _keycloakService.SendPasswordResetEmailAsync(request.ResetDto.UserEmail);

                if (!result)
                {
                    throw new Exception("Hubo un problema al enviar el correo de recuperación de contraseña.");
                }

                // 🔹 Registrar la actividad si el proceso fue exitoso
                var activity = new Domain.Entities.ActivityHistory.ActivityHistory(
                    Guid.NewGuid(),
                    Id,
                    "Solicitud de recuperación de contraseña",
                    DateTime.UtcNow
                );

                await _activityHistoryRepository.AddAsync(activity);
                var activityDto = _mapper.Map<GetActivityHistoryDto>(activity);
                await _eventBusActivity.PublishMessageAsync(activityDto, "activityHistoryQueue", "PASSWORD_RECOVERY_REQUESTED");

              

                return result;
            }
            catch (HttpRequestException httpEx)
            {
              
                throw new Exception("No se pudo conectar con el servidor de autenticación. Inténtalo de nuevo más tarde.", httpEx);
            }
            catch (KeycloakException kcEx)
            {
               
                throw new Exception("Error interno en el sistema de autenticación. Contacta al soporte.", kcEx);
            }
            catch (Exception ex)
            {
               
                throw new Exception("Se produjo un error inesperado en el proceso de recuperación de contraseña.", ex);
            }
        }

    }
}
