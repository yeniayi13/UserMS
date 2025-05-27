using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Application.Commands.Keycloak;
using UserMs.Core.Service.Keycloak;

namespace UserMs.Application.Handlers.Keycloak
{
    public class LogOutCommandHandler : IRequestHandler<LogOutCommand, string>
    {
        private readonly IKeycloakService _keycloakService;

        public LogOutCommandHandler(IKeycloakService keycloakService)
        {
            _keycloakService = keycloakService;
        }

        public async Task<string> Handle(LogOutCommand request, CancellationToken cancellationToken)
        {
           

            try
            {
                // 🔹 Cerrar sesión en Keycloak con el UserId
                var result = await _keycloakService.LogOutAsync();

                if (string.IsNullOrEmpty(result))
                {
                    throw new Exception($"Hubo un problema al cerrar sesión para el usuario ");
                }

                

                return $"Usuario cerró sesión correctamente.";
            }
            catch (HttpRequestException httpEx)
            {
                
                throw new Exception("No se pudo conectar con el servidor de autenticación. Inténtalo de nuevo más tarde.", httpEx);
            }
            catch (Exception ex)
            {
                
                throw new Exception("Se produjo un error inesperado al intentar cerrar sesión.", ex);
            }
        }
    }
}
