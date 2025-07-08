using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserMs.Application.Commands.Auctioneer;
using UserMs.Application.Queries.Auctioneer;
using UserMs.Application.Queries.Bidder;
using UserMs.Commoon.Dtos.Users.Request.Auctioneer;
using UserMs.Domain.Entities;
using UserMs.Domain.Entities.Auctioneer.ValueObjects;

namespace UserMs.Controllers
{
    [ApiController]
    [Route("user/auctioneer")]
    public class AuctioneerController : ControllerBase
    {
        private readonly ILogger<AuctioneerController> _logger;
        private readonly IMediator _mediator;

        public AuctioneerController(ILogger<AuctioneerController> logger, IMediator mediator)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        [HttpPost("Auctioneer-Registration")]
        public async Task<IActionResult> CreateAuctioneer([FromBody] CreateAuctioneerDto createAuctioneerDto)
        {
            if (createAuctioneerDto == null)
            {
                _logger.LogWarning("Solicitud de creación de subastador con datos vacíos.");
                return BadRequest("Los datos del subastador no pueden estar vacíos.");
            }

            try
            {
                var command = new CreateAuctioneerCommand(createAuctioneerDto);
                var auctioneerId = await _mediator.Send(command);
                return Ok(new { AuctioneerId = auctioneerId, Message = "Subastador registrado exitosamente." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al intentar registrar un subastador.");
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error interno al crear el subastador. {ex.Message}");
            }
        }
       // [Authorize]
        [HttpGet("Auctioneer-All")]
        public async Task<IActionResult> GetAuctioneer()
        {
            try
            {
                var query = new GetAuctioneerAllQuery();
                var auctioneers = await _mediator.Send(query);

                if (auctioneers == null || !auctioneers.Any())
                {
                    _logger.LogWarning("No se encontraron subastadores en el sistema.");
                    return NotFound("No se encontraron subastadores.");
                }

                return Ok(auctioneers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al obtener los subastadores.");
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error interno al obtener los subastadores. {ex.Message}");
            }
        }
        //[Authorize]
        [HttpGet("{auctioneerId}")]
        public async Task<IActionResult> GetAuctioneerById([FromRoute] Guid auctioneerId)
        {
            if (auctioneerId == Guid.Empty)
            {
                _logger.LogWarning("Solicitud con ID de subastador vacío.");
                return BadRequest("El ID del subastador no puede estar vacío.");
            }

            try
            {
                var query = new GetAuctioneerByIdQuery(auctioneerId);
                var auctioneer = await _mediator.Send(query);

                if (auctioneer == null)
                {
                    _logger.LogWarning($"No se encontró un subastador con ID: {auctioneerId}");
                    return NotFound($"No se encontró un subastador con ID: {auctioneerId}");
                }

                return Ok(auctioneer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error inesperado al obtener el subastador con ID: {auctioneerId}");
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error interno al buscar el subastador.{ex.Message}");
            }
        }
       // [Authorize]
        [HttpGet("UserEmail/{UserEmail}")]
        public async Task<IActionResult> GetAuctioneerByUserEmail([FromRoute] string UserEmail)
        {
            if (string.IsNullOrWhiteSpace(UserEmail) || !UserEmail.Contains("@"))
            {
                _logger.LogWarning("Solicitud con correo electrónico inválido.");
                return BadRequest("El correo electrónico no es válido.");
            }

            try
            {
                var query = new GetAuctioneerByUserEmailQuery(UserEmail);
                var auctioneer = await _mediator.Send(query);

                if (auctioneer == null)
                {
                    _logger.LogWarning($"No se encontró un subastador con correo electrónico: {UserEmail}");
                    return NotFound($"No se encontró un subastador con correo electrónico: {UserEmail}");
                }

                return Ok(auctioneer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error inesperado al obtener el subastador con correo electrónico: {UserEmail}");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error interno al buscar el subastador.");
            }
        }
        [Authorize]
        [HttpPut("Update-Auctioneer/{auctioneerId}")]
        public async Task<IActionResult> UpdateAuctioneer([FromRoute] Guid auctioneerId, [FromBody] UpdateAuctioneerDto auctioneerDto)
        {
            if (auctioneerId == Guid.Empty || auctioneerDto == null)
            {
                _logger.LogWarning("Solicitud de actualización con datos inválidos.");
                return BadRequest("El ID del subastador y los datos de actualización no pueden estar vacíos.");
            }

            try
            {
                var command = new UpdateAuctioneerCommand(UserId.Create(auctioneerId), auctioneerDto);
                var auctioneer = await _mediator.Send(command); // Devuelve el objeto Auctioneers
                return Ok(auctioneer); // Ahora devuelve Auctioneers en lugar del mensaje de éxito
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error inesperado al actualizar el subastador con ID: {auctioneerId}");
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error interno al actualizar el subastador.{ex.Message}");
            }
        }
        [Authorize]
        [HttpDelete("Delete-Auctioneer/{auctioneerId}")]
        public async Task<IActionResult> DeleteAuctioneer([FromRoute] Guid auctioneerId)
        {
            if (auctioneerId == Guid.Empty)
            {
                _logger.LogWarning("Solicitud con ID de subastador vacío.");
                return BadRequest("El ID del subastador no puede estar vacío.");
            }

            try
            {
                var command = new DeleteAuctioneerCommand(UserId.Create(auctioneerId));
                await _mediator.Send(command);

                return Ok(auctioneerId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error inesperado al eliminar el subastador con ID: {auctioneerId}");
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error interno al eliminar el subastador.{ex.Message}s");
            }
        }
    }
}
