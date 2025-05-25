

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using UserMs.Core;
using UserMs.Commoon.Dtos;
using AuthMs.Common.Exceptions;
using UserMs.Infrastructure.Exceptions;
using UserMs.Commoon.Dtos.Keycloak;
using UserMs.Core.Service.Keycloak;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using UserMs.Domain.Entities.UserEntity;
using System.Net.Http.Headers;
using UserMs.Infrastructure.Service.Keycloak;
using UserMs.Commoon.Dtos.Users.Response.ActivityHistory;
using UserMs.Core.RabbitMQ;
using UserMs.Core.Repositories.ActivityHistoryRepo;
using MediatR;
using UserMs.Application.Commands.ActivityHistory;
using UserMs.Application.Commands.Keycloak;


namespace UserMs.Controllers
{

    [ApiController]
    [Route("user/auth")]
    public class AuthController : ControllerBase
    {
        private readonly ILogger<AuthController> _logger;
        private readonly IMediator _mediator;

        public AuthController(ILogger<AuthController> logger, IMediator mediator)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync([FromBody] LoginDto request)
        {
            if (request == null)
            {
                _logger.LogWarning("Solicitud de inicio de sesi�n con cuerpo vac�o.");
                return BadRequest("El cuerpo de la solicitud no puede estar vac�o.");
            }

            try
            {
                var command = new LoginCommand(request);
                var token = await _mediator.Send(command);

                return Ok(token);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Credenciales incorrectas.");
                return Unauthorized("Credenciales incorrectas.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al intentar iniciar sesi�n.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error interno al procesar el inicio de sesi�n.");
            }
        }

        [HttpPost("logout")]
        public async Task<IActionResult> LogOutAsync()
        {
            try
            {
                var command = new LogOutCommand();
                var result = await _mediator.Send(command);

                return Ok("Sesi�n cerrada correctamente.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al cerrar sesi�n.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error interno al cerrar sesi�n.");
            }
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPasswordAsync([FromBody] ResetPasswordDto request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.UserEmail))
            {
                _logger.LogWarning("Solicitud de restablecimiento de contrase�a con datos inv�lidos.");
                return BadRequest("El correo electr�nico no puede estar vac�o.");
            }

            try
            {
                var command = new ResetPasswordCommand(request);
                var result = await _mediator.Send(command);

                return Ok("Correo de recuperaci�n enviado exitosamente.");
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogError(ex, $"El correo electr�nico {request.UserEmail} no est� registrado.");
                return NotFound("El correo electr�nico ingresado no est� registrado.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al restablecer la contrase�a.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error interno al procesar el restablecimiento de contrase�a.");
            }
        }
    }
}