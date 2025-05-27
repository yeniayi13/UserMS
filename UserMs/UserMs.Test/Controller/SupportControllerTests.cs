using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using UserMs.Application.Commands.Support;
using UserMs.Application.Queries.Support;
using UserMs.Commoon.Dtos.Users.Request.Support;
using UserMs.Commoon.Dtos.Users.Response.Support;
using UserMs.Controllers;
using UserMs.Domain.Entities.Support;
using UserMs.Domain.Entities.Support.ValueObjects;
using Xunit;
namespace UserMs.Test.Controller
{
    

    public class SupportControllerTests
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly Mock<ILogger<SupportController>> _mockLogger;
        private readonly SupportController _controller;

        public SupportControllerTests()
        {
            _mockMediator = new Mock<IMediator>();
            _mockLogger = new Mock<ILogger<SupportController>>();
            _controller = new SupportController(_mockLogger.Object, _mockMediator.Object);
        }

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenLoggerIsNull()
        {
            var exception = Assert.Throws<ArgumentNullException>(() => new SupportController(null, _mockMediator.Object));
            Assert.Equal("logger", exception.ParamName);
        }

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenMediatorIsNull()
        {
            var exception = Assert.Throws<ArgumentNullException>(() => new SupportController(_mockLogger.Object, null));
            Assert.Equal("mediator", exception.ParamName);
        }

        [Fact]
        public async Task CreateSupport_ReturnsBadRequest_WhenDtoIsNull()
        {
            var result = await _controller.CreateSupport(null) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("Los datos del soporte no pueden estar vacíos.", result.Value);
        }

        [Fact]
        public async Task GetSupport_ReturnsNotFound_WhenNoSupportsExist()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetSupportAllQuery>(), default)).ReturnsAsync((List<GetSupportDto>)null);
            var result = await _controller.GetSupport() as NotFoundObjectResult;
            Assert.NotNull(result);
            Assert.Equal(404, result.StatusCode);
            Assert.Equal("No se encontraron soportes.", result.Value);
        }

        [Fact]
        public async Task GetSupportById_ReturnsBadRequest_WhenSupportIdIsEmpty()
        {
            var result = await _controller.GetSupportById(Guid.Empty) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("El ID del soporte no puede estar vacío.", result.Value);
        }

        [Fact]
        public async Task GetSupportByUserEmail_ReturnsBadRequest_WhenEmailIsEmpty()
        {
            var result = await _controller.GetSupportByUserEmail("") as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("El correo electrónico no puede estar vacío.", result.Value);
        }

        [Fact]
        public async Task UpdateSupport_ReturnsBadRequest_WhenSupportIdIsEmpty()
        {
            var result = await _controller.UpdateSupport(Guid.Empty, new UpdateSupportDto()) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("El ID del soporte y los datos de actualización no pueden estar vacíos.", result.Value);
        }

        [Fact]
        public async Task UpdateSupport_ReturnsOk_WhenSupportIsUpdated()
        {
            Guid supportId = Guid.NewGuid();
            var supportDto = new UpdateSupportDto();
            var expectedSupport = new Supports { UserId = supportId };
            var dto = new GetSupportDto
            {
                UserId = expectedSupport.UserId.Value,
                
            };

            _mockMediator.Setup(m => m.Send(It.IsAny<UpdateSupportCommand>(), default))
                .ReturnsAsync(dto);

            var result = await _controller.UpdateSupport(supportId, supportDto) as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal(dto, result.Value);
        }

        [Fact]
        public async Task DeleteSupport_ReturnsBadRequest_WhenSupportIdIsEmpty()
        {
            var result = await _controller.DeleteSupport(Guid.Empty) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("El ID del soporte no puede estar vacío.", result.Value);
        }

        [Fact]
        public async Task DeleteSupport_ReturnsOk_WhenSupportIsDeleted()
        {
            Guid supportId = Guid.NewGuid();

            _mockMediator.Setup(m => m.Send(It.IsAny<DeleteSupportCommand>(), default))
                .ReturnsAsync(supportId);

            var result = await _controller.DeleteSupport(supportId) as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal(supportId, result.Value);
        }

        [Fact]
        public async Task CreateSupport_ReturnsInternalServerError_WhenExceptionOccurs()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<CreateSupportCommand>(), default))
                         .ThrowsAsync(new Exception("Unexpected error"));

            var result = await _controller.CreateSupport(new CreateSupportDto()) as ObjectResult;

            Assert.NotNull(result);
            Assert.Equal(500, result.StatusCode);
            Assert.Equal("Error interno al crear el soporte.", result.Value);
        }

        [Fact]
        public async Task GetSupport_ReturnsInternalServerError_WhenExceptionOccurs()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetSupportAllQuery>(), default))
                         .ThrowsAsync(new Exception("Unexpected error"));

            var result = await _controller.GetSupport() as ObjectResult;

            Assert.NotNull(result);
            Assert.Equal(500, result.StatusCode);
            Assert.Equal("Error interno al obtener los soportes.", result.Value);
        }

        [Fact]
        public async Task GetSupportById_ReturnsInternalServerError_WhenExceptionOccurs()
        {
            Guid supportId = Guid.NewGuid();

            _mockMediator.Setup(m => m.Send(It.IsAny<GetSupportByIdQuery>(), default))
                         .ThrowsAsync(new Exception("Unexpected error"));

            var result = await _controller.GetSupportById(supportId) as ObjectResult;

            Assert.NotNull(result);
            Assert.Equal(500, result.StatusCode);
            Assert.Equal("Error interno al buscar el soporte.", result.Value);
        }

        [Fact]
        public async Task UpdateSupport_ReturnsInternalServerError_WhenExceptionOccurs()
        {
            Guid supportId = Guid.NewGuid();
            var supportDto = new UpdateSupportDto();

            _mockMediator.Setup(m => m.Send(It.IsAny<UpdateSupportCommand>(), default))
                         .ThrowsAsync(new Exception("Unexpected error"));

            var result = await _controller.UpdateSupport(supportId, supportDto) as ObjectResult;

            Assert.NotNull(result);
            Assert.Equal(500, result.StatusCode);
            Assert.Equal("Error interno al actualizar el soporte.", result.Value);
        }

        [Fact]
        public async Task DeleteSupport_ReturnsInternalServerError_WhenExceptionOccurs()
        {
            Guid supportId = Guid.NewGuid();

            _mockMediator.Setup(m => m.Send(It.IsAny<DeleteSupportCommand>(), default))
                         .ThrowsAsync(new Exception("Unexpected error"));

            var result = await _controller.DeleteSupport(supportId) as ObjectResult;

            Assert.NotNull(result);
            Assert.Equal(500, result.StatusCode);
            Assert.Equal("Error interno al eliminar el soporte.", result.Value);
        }

        [Fact]
        public async Task GetSupportById_ReturnsNotFound_WhenSupportDoesNotExist()
        {
            Guid supportId = Guid.NewGuid();
            _mockMediator.Setup(m => m.Send(It.IsAny<GetSupportByIdQuery>(), default))
                         .ReturnsAsync((GetSupportDto)null);

            var result = await _controller.GetSupportById(supportId) as NotFoundObjectResult;

            Assert.NotNull(result);
            Assert.Equal(404, result.StatusCode);
            Assert.Equal($"No se encontró un soporte con ID: {supportId}", result.Value);
        }

        [Fact]
        public async Task GetSupportByUserEmail_ReturnsNotFound_WhenSupportDoesNotExist()
        {
            string email = "notfound@example.com";
            _mockMediator.Setup(m => m.Send(It.IsAny<GetSupportByUserEmailQuery>(), default))
                         .ReturnsAsync((GetSupportDto)null);

            var result = await _controller.GetSupportByUserEmail(email) as NotFoundObjectResult;

            Assert.NotNull(result);
            Assert.Equal(404, result.StatusCode);
            Assert.Equal($"No se encontró soporte con el correo: {email}", result.Value);
        }

        [Fact]
        public async Task CreateSupport_ReturnsCreated_WhenSupportIsSuccessfullyCreated()
        {
            var supportDto = new CreateSupportDto();
            Guid supportId = Guid.NewGuid();

            _mockMediator.Setup(m => m.Send(It.IsAny<CreateSupportCommand>(), default))
                         .ReturnsAsync(supportId);

            var result = await _controller.CreateSupport(supportDto) as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
          
        }

        [Fact]
        public async Task GetSupport_ReturnsOk_WithMultipleSupports()
        {
            var supports = new List<GetSupportDto> { new GetSupportDto(), new GetSupportDto() };
            _mockMediator.Setup(m => m.Send(It.IsAny<GetSupportAllQuery>(), default))
                         .ReturnsAsync(supports);

            var result = await _controller.GetSupport() as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal(supports, result.Value);
        }
    }
}
