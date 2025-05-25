

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
                _logger.LogWarning("Solicitud de inicio de sesión con cuerpo vacío.");
                return BadRequest("El cuerpo de la solicitud no puede estar vacío.");
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
                _logger.LogError(ex, "Error inesperado al intentar iniciar sesión.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error interno al procesar el inicio de sesión.");
            }
        }

        [HttpPost("logout")]
        public async Task<IActionResult> LogOutAsync()
        {
            try
            {
                var command = new LogOutCommand();
                var result = await _mediator.Send(command);

                return Ok("Sesión cerrada correctamente.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al cerrar sesión.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error interno al cerrar sesión.");
            }
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPasswordAsync([FromBody] ResetPasswordDto request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.UserEmail))
            {
                _logger.LogWarning("Solicitud de restablecimiento de contraseña con datos inválidos.");
                return BadRequest("El correo electrónico no puede estar vacío.");
            }

            try
            {
                var command = new ResetPasswordCommand(request);
                var result = await _mediator.Send(command);

                return Ok("Correo de recuperación enviado exitosamente.");
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogError(ex, $"El correo electrónico {request.UserEmail} no está registrado.");
                return NotFound("El correo electrónico ingresado no está registrado.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al restablecer la contraseña.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error interno al procesar el restablecimiento de contraseña.");
            }
        }
    }
}