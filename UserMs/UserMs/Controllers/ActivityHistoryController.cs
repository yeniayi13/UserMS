using Microsoft.AspNetCore.Authorization;

namespace UserMs.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using MediatR;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Threading.Tasks;
    using UserMs.Application.Commands.ActivityHistory;
    using UserMs.Application.Queries.HistoryActivity;

    using Microsoft.AspNetCore.Mvc;
    using MediatR;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Threading.Tasks;
    using UserMs.Commoon.Dtos.Users.Request.ActivityHistory;

    [ApiController]
    [Route("user/activity-history")]
    public class ActivityHistoryController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<ActivityHistoryController> _logger;

        public ActivityHistoryController(IMediator mediator, ILogger<ActivityHistoryController> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [Authorize]
        [HttpGet("UserId/{UserId}")]
        public async Task<IActionResult> GetActivitiesByUserId(
            [FromRoute] Guid UserId,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate)
        {
            if (UserId == Guid.Empty)
            {
                _logger.LogWarning("Solicitud con ID de usuario vacío.");
                return BadRequest("El ID del usuario no puede estar vacío.");
            }

            try
            {
                var query = new GetActivitiesByUserQuery(UserId, startDate, endDate);
                var activityHistory = await _mediator.Send(query);

                if (activityHistory == null || !activityHistory.Any())
                {
                    _logger.LogWarning($"No se encontraron actividades para el usuario con ID: {UserId}");
                    return NotFound($"No se encontraron actividades para el usuario con ID: {UserId}");
                }

                return Ok(activityHistory);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogError(ex, $"No se encontraron actividades para el usuario con ID: {UserId}");
                return NotFound($"No se encontraron actividades para el usuario con ID: {UserId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error inesperado al obtener el historial de actividades para el usuario con ID: {UserId}");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error interno al obtener el historial de actividades.");
            }
        }

        [HttpPost("UserId/{UserId}")]
        public async Task<IActionResult> CreateActivityHistory(
            [FromRoute] Guid UserId,
            [FromBody] CreateActivityHistoryDto historyActivityDto)
        {
            if (UserId == Guid.Empty)
            {
                _logger.LogWarning("Solicitud con ID de usuario vacío.");
                return BadRequest("El ID del usuario no puede estar vacío.");
            }

            if (historyActivityDto == null)
            {
                _logger.LogWarning("Intento de creación de actividad con datos nulos.");
                return BadRequest("La actividad no puede estar vacía.");
            }

            try
            {
                var command = new CreateHistoryActivityCommand(historyActivityDto);
                var createdActivityUserId = await _mediator.Send(command);

                return Ok(new { UserId = createdActivityUserId, Message = "Actividad registrada exitosamente." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error inesperado al registrar actividad para el usuario con ID: {UserId}");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error interno al registrar actividad.");
            }
        }
    }
}
