using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserMs.Application.Commands.Bidder;
using UserMs.Application.Queries.Bidder;
using UserMs.Commoon.Dtos.Users.Request.Bidder;
using UserMs.Domain.Entities;
using UserMs.Domain.Entities.Bidder.ValueObjects;

namespace UserMs.Controllers
{
    [ApiController]
    [Route("user/bidder")]
    public class BidderController : ControllerBase
    {
        private readonly ILogger<BidderController> _logger;
        private readonly IMediator _mediator;

        public BidderController(ILogger<BidderController> logger, IMediator mediator)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        [HttpPost("Bidder-Registration")]
        public async Task<IActionResult> CreateBidder([FromBody] CreateBidderDto createBidderDto)
        {
            if (createBidderDto == null)
            {
                _logger.LogWarning("Solicitud de creación de postor con datos vacíos.");
                return BadRequest("Los datos del postor no pueden estar vacíos.");
            }

            try
            {
                var command = new CreateBidderCommand(createBidderDto);
                var bidderId = await _mediator.Send(command);
                return Ok(new { BidderId = bidderId, Message = "Postor registrado exitosamente." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al intentar registrar un postor.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error interno al crear el postor.");
            }
        }
       // [Authorize]
        [HttpGet("Bidder-All")]
        public async Task<IActionResult> GetBidders()
        {
            try
            {
                var query = new GetBidderAllQuery();
                var bidders = await _mediator.Send(query);

                if (bidders == null || !bidders.Any())
                {
                    _logger.LogWarning("No se encontraron postores en el sistema.");
                    return NotFound("No se encontraron postores.");
                }

                return Ok(bidders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al obtener los postores.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error interno al obtener los postores.");
            }
        }
       // [Authorize]
        [HttpGet("{bidderId}")]
        public async Task<IActionResult> GetBidderById([FromRoute] Guid bidderId)
        {
            if (bidderId == Guid.Empty)
            {
                _logger.LogWarning("Solicitud con ID de postor vacío.");
                return BadRequest("El ID del postor no puede estar vacío.");
            }

            try
            {
                var query = new GetBidderByIdQuery(bidderId);
                var bidder = await _mediator.Send(query);

                if (bidder == null)
                {
                    _logger.LogWarning($"No se encontró un postor con ID: {bidderId}");
                    return NotFound($"No se encontró un postor con ID: {bidderId}");
                }

                return Ok(bidder);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error inesperado al obtener el postor con ID: {bidderId}");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error interno al buscar el postor.");
            }
        }
       // [Authorize]
        [HttpGet("UserEmail/{UserEmail}")]
        public async Task<IActionResult> GetBidderByUserEmail([FromRoute] string UserEmail)
        {
            if (string.IsNullOrWhiteSpace(UserEmail) || !UserEmail.Contains("@"))
            {
                _logger.LogWarning("Solicitud con correo electrónico inválido.");
                return BadRequest("El correo electrónico no es válido.");
            }

            try
            {
                var query = new GetBidderByUserEmailQuery(UserEmail);
                var bidder = await _mediator.Send(query);

                if (bidder == null)
                {
                    _logger.LogWarning($"No se encontró un postor con correo electrónico: {UserEmail}");
                    return NotFound($"No se encontró un postor con correo electrónico: {UserEmail}");
                }

                return Ok(bidder);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error inesperado al obtener el postor con correo electrónico: {UserEmail}");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error interno al buscar el postor.");
            }
        }
        [Authorize]
        [HttpPut("Update-Bidder/{bidderId}")]
        public async Task<IActionResult> UpdateBidder([FromRoute] Guid bidderId, [FromBody] UpdateBidderDto bidderDto)
        {
            if (bidderId == Guid.Empty || bidderDto == null)
            {
                _logger.LogWarning("Solicitud de actualización con datos inválidos.");
                return BadRequest("El ID del postor y los datos de actualización no pueden estar vacíos.");
            }

            try
            {
                var command = new UpdateBidderCommand(UserId.Create(bidderId), bidderDto);
                var bidder = await _mediator.Send(command);
                return Ok(bidder);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error inesperado al actualizar el postor con ID: {bidderId}");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error interno al actualizar el postor.");
            }
        }
        [Authorize]
        [HttpDelete("Delete-Bidder/{bidderId}")]
        public async Task<IActionResult> DeleteBidder([FromRoute] Guid bidderId)
        {
            if (bidderId == Guid.Empty)
            {
                _logger.LogWarning("Solicitud con ID de postor vacío.");
                return BadRequest("El ID del postor no puede estar vacío.");
            }

            try
            {
                var command = new DeleteBidderCommand(UserId.Create(bidderId));
                var result = await _mediator.Send(command);

                if (result == null) // Verifica si no existe el postor
                {
                    _logger.LogWarning($"No se encontró un postor con ID: {bidderId}");
                    return NotFound($"No se encontró un postor con ID: {bidderId}");
                }

                return Ok(bidderId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error inesperado al eliminar el postor con ID: {bidderId}");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error interno al eliminar el postor.");
            }
        }
    }
}
