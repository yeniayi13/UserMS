using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserMs.Test.Controller
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Moq;
    using UserMs.Application.Queries.User;
    using UserMs.Commoon.Dtos.Users.Response.User;
    using UserMs.Controllers;
    using Xunit;

    public class UsersControllerTests
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly Mock<ILogger<UsersController>> _mockLogger;
        private readonly UsersController _controller;

        public UsersControllerTests()
        {
            _mockMediator = new Mock<IMediator>();
            _mockLogger = new Mock<ILogger<UsersController>>();
            _controller = new UsersController(_mockLogger.Object, _mockMediator.Object);
        }

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenLoggerIsNull()
        {
            var exception = Assert.Throws<ArgumentNullException>(() => new UsersController(null, _mockMediator.Object));
            Assert.Equal("logger", exception.ParamName);
        }

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenMediatorIsNull()
        {
            var exception = Assert.Throws<ArgumentNullException>(() => new UsersController(_mockLogger.Object, null));
            Assert.Equal("mediator", exception.ParamName);
        }

        [Fact]
        public async Task GetUsers_ReturnsNotFound_WhenNoUsersExist()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetUsersQuery>(), default))
                         .ReturnsAsync((List<GetUsersDto>)null);

            var result = await _controller.GetUsers() as NotFoundObjectResult;

            Assert.NotNull(result);
            Assert.Equal(404, result.StatusCode);
            Assert.Equal("No se encontraron usuarios.", result.Value);
        }

        [Fact]
        public async Task GetUsers_ReturnsOk_WhenUsersExist()
        {
            var users = new List<GetUsersDto> { new GetUsersDto() };

            _mockMediator.Setup(m => m.Send(It.IsAny<GetUsersQuery>(), default))
                         .ReturnsAsync(users);

            var result = await _controller.GetUsers() as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal(users, result.Value);
        }

        [Fact]
        public async Task GetUsersById_ReturnsBadRequest_WhenUserIdIsEmpty()
        {
            var result = await _controller.GetUsersById(Guid.Empty) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("El ID del usuario no puede estar vacío.", result.Value);
        }

        [Fact]
        public async Task GetUsersById_ReturnsNotFound_WhenUserDoesNotExist()
        {
            Guid usersId = Guid.NewGuid();
            _mockMediator.Setup(m => m.Send(It.IsAny<GetUsersByIdQuery>(), default))
                         .ReturnsAsync((GetUsersDto)null);

            var result = await _controller.GetUsersById(usersId) as NotFoundObjectResult;

            Assert.NotNull(result);
            Assert.Equal(404, result.StatusCode);
            Assert.Equal($"No se encontró un usuario con ID: {usersId}", result.Value);
        }

        [Fact]
        public async Task GetUsersById_ReturnsOk_WhenUserExists()
        {
            Guid usersId = Guid.NewGuid();
            var user = new GetUsersDto();

            _mockMediator.Setup(m => m.Send(It.IsAny<GetUsersByIdQuery>(), default))
                         .ReturnsAsync(user);

            var result = await _controller.GetUsersById(usersId) as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal(user, result.Value);
        }

        [Fact]
        public async Task GetUsers_ReturnsInternalServerError_WhenExceptionOccurs()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetUsersQuery>(), default))
                         .ThrowsAsync(new Exception("Unexpected error"));

            var result = await _controller.GetUsers() as ObjectResult;

            Assert.NotNull(result);
            Assert.Equal(500, result.StatusCode);
            Assert.Equal("Error interno al obtener los usuarios.", result.Value);
        }

        [Fact]
        public async Task GetUsersById_ReturnsInternalServerError_WhenExceptionOccurs()
        {
            Guid usersId = Guid.NewGuid();

            _mockMediator.Setup(m => m.Send(It.IsAny<GetUsersByIdQuery>(), default))
                         .ThrowsAsync(new Exception("Unexpected error"));

            var result = await _controller.GetUsersById(usersId) as ObjectResult;

            Assert.NotNull(result);
            Assert.Equal(500, result.StatusCode);
            Assert.Equal("Error interno al buscar el usuario.", result.Value);
        }
    }
}
