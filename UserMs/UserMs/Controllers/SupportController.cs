using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserMs.Application.Commands.Support;
using UserMs.Application.Queries.Auctioneer;
using UserMs.Application.Queries.Support;
using UserMs.Commoon.Dtos.Users.Request.Support;
using UserMs.Domain.Entities;
using UserMs.Domain.Entities.Support.ValueObjects;

namespace UserMs.Controllers
{
    [ApiController]
    [Route("user/support")]
    public class SupportController : ControllerBase
    {
        private readonly ILogger<SupportController> _logger;
        private readonly IMediator _mediator;

        public SupportController(ILogger<SupportController> logger, IMediator mediator)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }
       // [Authorize]
        [HttpPost("Support-Registration")]
        public async Task<IActionResult> CreateSupport([FromBody] CreateSupportDto createSupportDto)
        {
            if (createSupportDto == null)
            {
                _logger.LogWarning("Solicitud de creación de soporte con datos vacíos.");
                return BadRequest("Los datos del soporte no pueden estar vacíos.");
            }

            try
            {
                var command = new CreateSupportCommand(createSupportDto);
                var supportId = await _mediator.Send(command);
                return Ok(new { SupportId = supportId, Message = "Soporte registrado exitosamente." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al intentar registrar un soporte.");
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error interno al crear el soporte. {ex.Message}");
            }
        }
        //[Authorize]
        [HttpGet("Support-All")]
        public async Task<IActionResult> GetSupport()
        {
            try
            {
                var query = new GetSupportAllQuery();
                var supports = await _mediator.Send(query);

                if (supports == null || !supports.Any())
                {
                    _logger.LogWarning("No se encontraron registros de soporte.");
                    return NotFound("No se encontraron soportes.");
                }

                return Ok(supports);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al obtener los registros de soporte.");
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error interno al obtener los soportes. {ex.Message}");
            }
        }
       // [Authorize]
        [HttpGet("{supportId}")]
        public async Task<IActionResult> GetSupportById([FromRoute] Guid supportId)
        {
            if (supportId == Guid.Empty)
            {
                _logger.LogWarning("Solicitud con ID de soporte vacío.");
                return BadRequest("El ID del soporte no puede estar vacío.");
            }

            try
            {
                var query = new GetSupportByIdQuery(supportId);
                var support = await _mediator.Send(query);

                if (support == null)
                {
                    _logger.LogWarning($"No se encontró un soporte con ID: {supportId}");
                    return NotFound($"No se encontró un soporte con ID: {supportId}");
                }

                return Ok(support);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error inesperado al obtener el soporte con ID: {supportId}");
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error interno al buscar el soporte.{ex.Message}");
            }
        }
        //[Authorize]
        [HttpGet("UserEmail/{UserEmail}")]
        public async Task<IActionResult> GetSupportByUserEmail([FromRoute] string UserEmail)
        {
            if (string.IsNullOrWhiteSpace(UserEmail))
            {
                _logger.LogWarning("Solicitud con correo electrónico vacío.");
                return BadRequest("El correo electrónico no puede estar vacío.");
            }

            try
            {
                var query = new GetSupportByUserEmailQuery(UserEmail);
                var support = await _mediator.Send(query);

                if (support == null)
                {
                    _logger.LogWarning($"No se encontró soporte con el correo: {UserEmail}");
                    return NotFound($"No se encontró soporte con el correo: {UserEmail}");
                }

                return Ok(support);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error inesperado al obtener el soporte con correo: {UserEmail}");
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error interno al buscar el soporte.{ex.Message}");
            }
        }
        [Authorize]
        [HttpPut("Update-Support/{supportId}")]
        public async Task<IActionResult> UpdateSupport([FromRoute] Guid supportId, [FromBody] UpdateSupportDto supportDto)
        {
            if (supportId == Guid.Empty || supportDto == null)
            {
                _logger.LogWarning("Solicitud de actualización con datos inválidos.");
                return BadRequest("El ID del soporte y los datos de actualización no pueden estar vacíos.");
            }

            try
            {
                var command = new UpdateSupportCommand(UserId.Create(supportId), supportDto);
                var support = await _mediator.Send(command);
                return Ok(support);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error inesperado al actualizar el soporte con ID: {supportId}");
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error interno al actualizar el soporte.{ex.Message}");
            }
        }
        [Authorize]
        [HttpDelete("Delete-Support/{supportId}")]
        public async Task<IActionResult> DeleteSupport([FromRoute] Guid supportId)
        {
            if (supportId == Guid.Empty)
            {
                _logger.LogWarning("Solicitud con ID de soporte vacío.");
                return BadRequest("El ID del soporte no puede estar vacío.");
            }

            try
            {
                var command = new DeleteSupportCommand(UserId.Create(supportId));
                await _mediator.Send(command);

                return Ok(supportId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error inesperado al eliminar el soporte con ID: {supportId}");
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error interno al eliminar el soporte.{ex.Message}");
            }
        }
    }
}
