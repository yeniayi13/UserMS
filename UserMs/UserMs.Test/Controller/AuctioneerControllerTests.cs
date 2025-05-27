using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Application.Commands.Auctioneer;
using UserMs.Commoon.Dtos.Users.Response.Auctioneer;
using UserMs.Domain.Entities;
using UserMs.Domain.Entities.Auctioneer;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using UserMs.Application.Queries.Auctioneer;
using UserMs.Commoon.Dtos.Users.Request.Auctioneer;
using UserMs.Controllers;
using UserMs.Domain.Entities.Auctioneer.ValueObjects;
using UserMs.Domain.Entities.IUser.ValueObjects;
using Xunit;
using UserMs.Commoon.Dtos.Users.Response.Bidder;

namespace UserMs.Test.Controller
{
   
    public class AuctioneerControllerTests
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly Mock<ILogger<AuctioneerController>> _mockLogger;
        private readonly AuctioneerController _controller;

        public AuctioneerControllerTests()
        {
            _mockMediator = new Mock<IMediator>();
            _mockLogger = new Mock<ILogger<AuctioneerController>>();
            _controller = new AuctioneerController(_mockLogger.Object, _mockMediator.Object);
        }

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenLoggerIsNull()
        {
            var exception = Assert.Throws<ArgumentNullException>(() => new AuctioneerController(null, _mockMediator.Object));
            Assert.Equal("logger", exception.ParamName);
        }

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenMediatorIsNull()
        {
            var exception = Assert.Throws<ArgumentNullException>(() => new AuctioneerController(_mockLogger.Object, null));
            Assert.Equal("mediator", exception.ParamName);
        }

        [Fact]
        public async Task CreateAuctioneer_ReturnsBadRequest_WhenDtoIsNull()
        {
            var result = await _controller.CreateAuctioneer(null) as BadRequestObjectResult;
            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("Los datos del subastador no pueden estar vacíos.", result.Value);
        }

        [Fact]
        public async Task GetAuctioneer_ReturnsNotFound_WhenNoAuctioneersExist()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetAuctioneerAllQuery>(), default)).ReturnsAsync((List<GetAuctioneerDto>)null);
            var result = await _controller.GetAuctioneer() as NotFoundObjectResult;
            Assert.NotNull(result);
            Assert.Equal(404, result.StatusCode);
            Assert.Equal("No se encontraron subastadores.", result.Value);
        }

        [Fact]
        public async Task GetAuctioneerById_ReturnsBadRequest_WhenAuctioneerIdIsEmpty()
        {
            var result = await _controller.GetAuctioneerById(Guid.Empty) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("El ID del subastador no puede estar vacío.", result.Value);
        }

        [Fact]
        public async Task GetAuctioneerById_ReturnsNotFound_WhenAuctioneerDoesNotExist()
        {
            Guid auctioneerId = Guid.NewGuid();
            _mockMediator.Setup(m => m.Send(It.IsAny<GetAuctioneerByIdQuery>(), default))
                         .ReturnsAsync((GetAuctioneerDto)null);

            var result = await _controller.GetAuctioneerById(auctioneerId) as NotFoundObjectResult;

            Assert.NotNull(result);
            Assert.Equal(404, result.StatusCode);
            Assert.Equal($"No se encontró un subastador con ID: {auctioneerId}", result.Value);
        }

        [Fact]
        public async Task GetAuctioneerById_ReturnsOk_WhenAuctioneerExists()
        {
            Guid auctioneerId = Guid.NewGuid();
            var mockAuctioneer = new GetAuctioneerDto();

            _mockMediator.Setup(m => m.Send(It.IsAny<GetAuctioneerByIdQuery>(), default))
                         .ReturnsAsync(mockAuctioneer);

            var result = await _controller.GetAuctioneerById(auctioneerId) as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal(mockAuctioneer, result.Value);
        }

        [Fact]
        public async Task GetAuctioneerByUserEmail_ReturnsBadRequest_WhenEmailIsEmpty()
        {
            var result = await _controller.GetAuctioneerByUserEmail("") as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("El correo electrónico no es válido.", result.Value);
        }

        [Fact]
        public async Task GetAuctioneerByUserEmail_ReturnsNotFound_WhenAuctioneerDoesNotExist()
        {
            string email = "notfound@example.com";
            _mockMediator.Setup(m => m.Send(It.IsAny<GetAuctioneerByUserEmailQuery>(), default))
                         .ReturnsAsync((GetAuctioneerDto)null);

            var result = await _controller.GetAuctioneerByUserEmail(email) as NotFoundObjectResult;

            Assert.NotNull(result);
            Assert.Equal(404, result.StatusCode);
            Assert.Equal($"No se encontró un subastador con correo electrónico: {email}", result.Value);
        }

        [Fact]
        public async Task UpdateAuctioneer_ReturnsBadRequest_WhenAuctioneerIdIsEmpty()
        {
            var result = await _controller.UpdateAuctioneer(Guid.Empty, new UpdateAuctioneerDto()) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("El ID del subastador y los datos de actualización no pueden estar vacíos.", result.Value);
        }
        [Fact]
        public async Task UpdateAuctioneer_ReturnsOk_WhenAuctioneerIsUpdated()
        {
            Guid auctioneerId = Guid.NewGuid();
            var auctioneerDto = new UpdateAuctioneerDto();
         
            var auctionerName = UserName.Create("Jose");
            var existingAuctioneer = new Auctioneers { UserId = auctioneerId, UserName = auctionerName };
            var dto = new GetAuctioneerDto
            {
                UserId = existingAuctioneer.UserId.Value,
                UserName = existingAuctioneer.UserName.Value,

            };
            _mockMediator.Setup(m => m.Send(It.IsAny<UpdateAuctioneerCommand>(), default))
                .ReturnsAsync(dto);

            var result = await _controller.UpdateAuctioneer(auctioneerId, auctioneerDto) as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal(dto, result.Value); // Comparación correcta del objeto Auctioneers
        }

        [Fact]
        public async Task DeleteAuctioneer_ReturnsOk_WhenAuctioneerIsDeleted()
        {
            Guid auctioneerId = Guid.NewGuid();
            ;

            _mockMediator.Setup(m => m.Send(It.IsAny<DeleteAuctioneerCommand>(), default))
                .ReturnsAsync(auctioneerId);

            var result = await _controller.DeleteAuctioneer(auctioneerId) as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal(auctioneerId, result.Value);
        }

        [Fact]
        public async Task DeleteAuctioneer_ReturnsBadRequest_WhenAuctioneerIdIsEmpty()
        {
            var result = await _controller.DeleteAuctioneer(Guid.Empty) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("El ID del subastador no puede estar vacío.", result.Value);
        }

        [Fact]
        public async Task CreateAuctioneer_ReturnsInternalServerError_WhenExceptionOccurs()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<CreateAuctioneerCommand>(), default))
                         .ThrowsAsync(new Exception("Unexpected error"));

            var result = await _controller.CreateAuctioneer(new CreateAuctioneerDto()) as ObjectResult;

            Assert.NotNull(result);
            Assert.Equal(500, result.StatusCode);
            Assert.Equal("Error interno al crear el subastador.", result.Value);
        }

        [Fact]
        public async Task GetAuctioneer_ReturnsInternalServerError_WhenExceptionOccurs()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetAuctioneerAllQuery>(), default))
                         .ThrowsAsync(new Exception("Unexpected error"));

            var result = await _controller.GetAuctioneer() as ObjectResult;

            Assert.NotNull(result);
            Assert.Equal(500, result.StatusCode);
            Assert.Equal("Error interno al obtener los subastadores.", result.Value);
        }

        [Fact]
        public async Task UpdateAuctioneer_ReturnsInternalServerError_WhenExceptionOccurs()
        {
            Guid auctioneerId = Guid.NewGuid();
            var auctioneerDto = new UpdateAuctioneerDto();

            _mockMediator.Setup(m => m.Send(It.IsAny<UpdateAuctioneerCommand>(), default))
                         .ThrowsAsync(new Exception("Unexpected error"));

            var result = await _controller.UpdateAuctioneer(auctioneerId, auctioneerDto) as ObjectResult;

            Assert.NotNull(result);
            Assert.Equal(500, result.StatusCode);
            Assert.Equal("Error interno al actualizar el subastador.", result.Value);
        }

        [Fact]
        public async Task DeleteAuctioneer_ReturnsInternalServerError_WhenExceptionOccurs()
        {
            Guid auctioneerId = Guid.NewGuid();

            _mockMediator.Setup(m => m.Send(It.IsAny<DeleteAuctioneerCommand>(), default))
                         .ThrowsAsync(new Exception("Unexpected error"));

            var result = await _controller.DeleteAuctioneer(auctioneerId) as ObjectResult;

            Assert.NotNull(result);
            Assert.Equal(500, result.StatusCode);
            Assert.Equal("Error interno al eliminar el subastador.", result.Value);
        }
        [Fact]
        public async Task GetAuctioneer_ReturnsOk_WithMultipleAuctioneers()
        {
            var auctioneers = new List<GetAuctioneerDto> { new GetAuctioneerDto(), new GetAuctioneerDto() };

            _mockMediator.Setup(m => m.Send(It.IsAny<GetAuctioneerAllQuery>(), default))
                .ReturnsAsync(auctioneers);

            var result = await _controller.GetAuctioneer() as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal(auctioneers, result.Value);
        }

        [Fact]
        public async Task GetAuctioneerById_ReturnsInternalServerError_WhenExceptionOccurs()
        {
            Guid auctioneerId = Guid.NewGuid();

            _mockMediator.Setup(m => m.Send(It.IsAny<GetAuctioneerByIdQuery>(), default))
                .ThrowsAsync(new Exception("Unexpected error"));

            var result = await _controller.GetAuctioneerById(auctioneerId) as ObjectResult;

            Assert.NotNull(result);
            Assert.Equal(500, result.StatusCode);
            Assert.Equal("Error interno al buscar el subastador.", result.Value);
        }

        [Fact]
        public async Task GetAuctioneerByUserEmail_ReturnsBadRequest_WhenEmailIsInvalid()
        {
            var result = await _controller.GetAuctioneerByUserEmail("invalid-email") as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("El correo electrónico no es válido.", result.Value);
        }

    }
}
