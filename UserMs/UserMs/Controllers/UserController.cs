using UserMs.Application.Commands.User;
using UserMs.Application.Queries.User;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using UserMs.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using UserMs.Commoon.Dtos.Users.Request;
using UserMs.Core.RabbitMQ;
using UserMs.Domain.Entities.IUser.ValueObjects;
using UserMs.Commoon.Dtos;


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
            _logger = logger;
            _mediator = mediator;
            
        }

        // [Authorize(Policy = "AuctioneerBidderOnly")]
        [HttpPost]
        public async Task<IActionResult> CreateUsers(CreateUsersDto createUsersDto)
        {
            try
            {
                // Enviamos el comando para agregar al usuario en PostgreSQL
                var command = new CreateUsersCommand(createUsersDto);
                var userId = await _mediator.Send(command);

                return Ok(userId);
            }
            catch (Exception e)
            {
                _logger.LogError("An error occurred while trying to create a User: {Message}", e.Message);
                return StatusCode(500, "An error occurred while trying to create a User");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            try
            {
                // Obtenemos todos los usuarios usando una consulta
                var query = new GetUsersQuery();
                var users = await _mediator.Send(query);
                return Ok(users);
            }
            catch (Exception e)
            {
                _logger.LogError("An error occurred while getting Users: {Message}", e.Message);
                return StatusCode(500, "An error occurred while getting Users");
            }
        }

        [HttpGet("{usersId}")]
        public async Task<IActionResult> GetUsersById([FromRoute] Guid usersId)
        {
            try
            {
                //UserId userId = UserId.Create(usersId);
                var query = new GetUsersByIdQuery(usersId);
                var users = await _mediator.Send(query);
                return Ok(users);
            }
            catch (Exception e)
            {
                _logger.LogError("An error occurred while getting one User: {Message}", e.Message);
                return StatusCode(500, "An error occurred while getting one User");
            }
        }

        [HttpPut("{usersId}")]
        public async Task<IActionResult> UpdateUsers([FromRoute] Guid usersId, [FromBody] UpdateUsersDto usersDto)
        {
            try
            {
                UserId userId = UserId.Create(usersId);
                var command = new UpdateUsersCommand(userId, usersDto);
                var users = await _mediator.Send(command);
                return Ok(users);
            }
            catch (Exception e)
            {
                _logger.LogError("An error occurred while trying to update a User: {Message}", e.Message);
                return StatusCode(500, "An error occurred while trying to update a User");
            }
        }

        [HttpDelete("{usersId}")]
        public async Task<IActionResult> DeleteUsers(Guid usersId)
        {
            try
            {
                UserId userId = UserId.Create(usersId);
                var command = new DeleteUsersCommand(userId);
                var users = await _mediator.Send(command);
                return Ok(users);
            }
            catch (Exception e)
            {
                _logger.LogError("An error occurred while trying to delete a User: {Message}", e.Message);
                return StatusCode(500, "An error occurred while trying to delete a User");
            }
        }
    }
}