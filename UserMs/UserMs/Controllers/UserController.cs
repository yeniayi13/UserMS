using UserMs.Application.Commands.User;
using UserMs.Application.Queries.User;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using UserMs.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using UserMs.Core.RabbitMQ;
using UserMs.Domain.Entities.IUser.ValueObjects;
using UserMs.Commoon.Dtos;
using UserMs.Commoon.Dtos.Users.Request.User;
using UserMs.Application.Commands.Bidder;
using UserMs.Commoon.Dtos.Users.Request.Bidder;
using UserMs.Commoon.Dtos.Keycloak;
using UserMs.Application.Commands.Users;


namespace UserMs.Controllers
{

    [ApiController]
    [Route("user/users")]
    public class UsersController : ControllerBase
    {
        private readonly ILogger<UsersController> _logger;
        private readonly IMediator _mediator;

        public UsersController(ILogger<UsersController> logger, IMediator mediator)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        // [Authorize(Policy = "AdministradorPolicy")]


        [Authorize]
        [HttpGet("All")]
        public async Task<IActionResult> GetUsers()
        {
            try
            {
                var query = new GetUsersQuery();
                var users = await _mediator.Send(query);

                if (users == null || !users.Any())
                {
                    _logger.LogWarning("No se encontraron usuarios en el sistema.");
                    return NotFound("No se encontraron usuarios.");
                }

                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al obtener los usuarios.");
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error interno al obtener los usuarios.{ex.Message}");
            }
        }
        [Authorize]
        [HttpGet("{usersId}")]
        public async Task<IActionResult> GetUsersById([FromRoute] Guid usersId)
        {
            if (usersId == Guid.Empty)
            {
                _logger.LogWarning("Solicitud con ID de usuario vacío.");
                return BadRequest("El ID del usuario no puede estar vacío.");
            }

            try
            {
                var query = new GetUsersByIdQuery(usersId);
                var users = await _mediator.Send(query);

                if (users == null)
                {
                    _logger.LogWarning($"No se encontró un usuario con ID: {usersId}");
                    return NotFound($"No se encontró un usuario con ID: {usersId}");
                }

                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error inesperado al obtener el usuario con ID: {usersId}");
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error interno al buscar el usuario.{ex.Message}");
            }
        }


        [Authorize]
        [HttpPut("Update-Password/{userId}")]
        public async Task<IActionResult> UpdatePassword([FromRoute] Guid userId, [FromBody] UpdatePasswordDto updateDto)
        {
            if (userId == Guid.Empty || updateDto == null)
            {
                _logger.LogWarning("Solicitud de actualización con datos inválidos.");
                return BadRequest("El ID del postor y los datos de actualización no pueden estar vacíos.");
            }

            try
            {
                var command = new UpdatePasswordCommand(updateDto,userId);
                var user = await _mediator.Send(command);
                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error inesperado al actualizar el postor con ID: {userId}");
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error interno al actualizar el postor. {ex.Message}");
            }
        }

    }
}