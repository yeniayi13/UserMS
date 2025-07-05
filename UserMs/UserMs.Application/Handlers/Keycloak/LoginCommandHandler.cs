using AuthMs.Common.Exceptions;
using AutoMapper;
using MediatR;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Application.Commands.Bidder;
using UserMs.Application.Commands.Keycloak;
using UserMs.Commoon.Dtos;
using UserMs.Commoon.Dtos.Users.Response.ActivityHistory;
using UserMs.Core.Database;
using UserMs.Core.RabbitMQ;
using UserMs.Core.Repositories.ActivityHistoryRepo;
using UserMs.Core.Repositories.UserRepo;
using UserMs.Core.Service.Keycloak;
using UserMs.Domain.Entities;
using UserMs.Infrastructure.Exceptions;

namespace UserMs.Application.Handlers.Keycloak
{
    public class LoginCommandHandler : IRequestHandler<LoginCommand, string>
    {
        private readonly IKeycloakService _keycloakService;
        private readonly IActivityHistoryRepository _activityHistoryRepository;
        private readonly IEventBus<GetActivityHistoryDto> _eventBusActivity;
        private readonly IMapper _mapper;
        private readonly IUserRepositoryMongo _userRepositoryMongo;
        public LoginCommandHandler(
            IKeycloakService keycloakService,
            IActivityHistoryRepository activityHistoryRepository,
            IEventBus<GetActivityHistoryDto> eventBusActivity,IMapper mapper, IUserRepositoryMongo userRepositoryMongo)

        {
            _keycloakService = keycloakService;
            _activityHistoryRepository = activityHistoryRepository;
            _eventBusActivity = eventBusActivity;
            _mapper = mapper;
            _userRepositoryMongo = userRepositoryMongo;
        }

        public async Task<string> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Login.Username) || string.IsNullOrWhiteSpace(request.Login.Password))
            {
                throw new ArgumentException("El nombre de usuario y la contraseña no pueden estar vacíos.");
            }

            try
            {
                // 🔹 Obtener el userId antes de iniciar sesión
                var user = await _userRepositoryMongo.GetUsersByEmail(request.Login.Username);
                if (user == null)
                {
                    throw new UserNotFoundException("No se encontró ningún usuario con el nombre de usuario proporcionado.");
                }

                // 🔹 Iniciar sesión en Keycloak
                var token = await _keycloakService.LoginAsync(request.Login.Username, request.Login.Password);

                if (string.IsNullOrWhiteSpace(token))
                {
                    throw new UnauthorizedAccessException("Credenciales incorrectas. No se pudo obtener el token.");
                }
                var activity = new Domain.Entities.ActivityHistory.ActivityHistory(
                    Guid.NewGuid(),
                    user.UserId.Value,
                    "Inicio Sesion",
                    DateTime.UtcNow
                );

                await _activityHistoryRepository.AddAsync(activity);
                var activityDto = _mapper.Map<GetActivityHistoryDto>(activity);
                await _eventBusActivity.PublishMessageAsync(activityDto, "activityHistoryQueue", "ACTIVITY_CREATED");

                return token;
            }
            catch (HttpRequestException httpEx)
            {
                
                throw new Exception("No se pudo conectar con el servidor de autenticación. Inténtalo de nuevo más tarde.", httpEx);
            }
            catch (UnauthorizedAccessException authEx)
            {
               
                throw new Exception("Credenciales incorrectas. Verifica tu usuario y contraseña.", authEx);
            }
            catch (Exception ex)
            {
               
                throw new Exception("Se produjo un error al intentar iniciar sesión.", ex);
            }
        }

    }
}
