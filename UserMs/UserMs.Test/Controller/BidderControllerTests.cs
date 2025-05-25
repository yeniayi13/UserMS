using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Commoon.Dtos.Users.Response.Bidder;
using UserMs.Domain.Entities.Bidder;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using UserMs.Application.Commands.Bidder;
using UserMs.Application.Queries.Bidder;
using UserMs.Commoon.Dtos.Users.Request.Bidder;
using UserMs.Controllers;
using UserMs.Domain.Entities;
using UserMs.Domain.Entities.Bidder.ValueObjects;
using UserMs.Domain.Entities.IUser.ValueObjects;
using Xunit;

namespace UserMs.Test.Controller
{
   

    public class BidderControllerTests
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly Mock<ILogger<BidderController>> _mockLogger;
        private readonly BidderController _controller;

        public BidderControllerTests()
        {
            _mockMediator = new Mock<IMediator>();
            _mockLogger = new Mock<ILogger<BidderController>>();
            _controller = new BidderController(_mockLogger.Object, _mockMediator.Object);
        }

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenLoggerIsNull()
        {
            var exception = Assert.Throws<ArgumentNullException>(() => new BidderController(null, _mockMediator.Object));
            Assert.Equal("logger", exception.ParamName);
        }

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenMediatorIsNull()
        {
            var exception = Assert.Throws<ArgumentNullException>(() => new BidderController(_mockLogger.Object, null));
            Assert.Equal("mediator", exception.ParamName);
        }

        [Fact]
        public async Task CreateBidder_ReturnsBadRequest_WhenDtoIsNull()
        {
            var result = await _controller.CreateBidder(null) as BadRequestObjectResult;
            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("Los datos del postor no pueden estar vacíos.", result.Value);
        }

        [Fact]
        public async Task GetBidders_ReturnsNotFound_WhenNoBiddersExist()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetBidderAllQuery>(), default)).ReturnsAsync((List<GetBidderDto>)null);
            var result = await _controller.GetBidders() as NotFoundObjectResult;
            Assert.NotNull(result);
            Assert.Equal(404, result.StatusCode);
            Assert.Equal("No se encontraron postores.", result.Value);
        }

        [Fact]
        public async Task GetBidderById_ReturnsBadRequest_WhenBidderIdIsEmpty()
        {
            var result = await _controller.GetBidderById(Guid.Empty) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("El ID del postor no puede estar vacío.", result.Value);
        }

        [Fact]
        public async Task GetBidderById_ReturnsNotFound_WhenBidderDoesNotExist()
        {
            Guid bidderId = Guid.NewGuid();
           
            _mockMediator.Setup(m => m.Send(It.IsAny<GetBidderByIdQuery>(), default))
                         .ReturnsAsync((GetBidderDto)null);

            var result = await _controller.GetBidderById(bidderId) as NotFoundObjectResult;

            Assert.NotNull(result);
            Assert.Equal(404, result.StatusCode);
            Assert.Equal($"No se encontró un postor con ID: {bidderId}", result.Value);
        }

        [Fact]
        public async Task GetBidderById_ReturnsOk_WhenBidderExists()
        {
            Guid bidderId = Guid.NewGuid();
            var mockBidder = new GetBidderDto() { };

            _mockMediator.Setup(m => m.Send(It.IsAny<GetBidderByIdQuery>(), default))
                         .ReturnsAsync(mockBidder);

            var result = await _controller.GetBidderById(bidderId) as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal(mockBidder, result.Value);
        }

        [Fact]
        public async Task GetBidderByUserEmail_ReturnsBadRequest_WhenEmailIsEmpty()
        {
            var result = await _controller.GetBidderByUserEmail("") as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("El correo electrónico no es válido.", result.Value);
        }

        [Fact]
        public async Task GetBidderByUserEmail_ReturnsNotFound_WhenBidderDoesNotExist()
        {
            string email = "notfound@example.com";
            var mockBidder = new GetBidderDto() { };
            _mockMediator.Setup(m => m.Send(It.IsAny<GetBidderByUserEmailQuery>(), default))
                         .ReturnsAsync((GetBidderDto)null);

            var result = await _controller.GetBidderByUserEmail(email) as NotFoundObjectResult;

            Assert.NotNull(result);
            Assert.Equal(404, result.StatusCode);
            Assert.Equal($"No se encontró un postor con correo electrónico: {email}", result.Value);
        }

        [Fact]
        public async Task UpdateBidder_ReturnsBadRequest_WhenBidderIdIsEmpty()
        {
            var result = await _controller.UpdateBidder(Guid.Empty, new UpdateBidderDto()) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("El ID del postor y los datos de actualización no pueden estar vacíos.", result.Value);
        }

        [Fact]
        public async Task UpdateBidder_ReturnsOk_WhenBidderIsUpdated()
        {
            Guid bidderId = Guid.NewGuid();
           
            var bidderName = UserName.Create("Name");
            var bidderDto = new UpdateBidderDto {  UserName = "Name" };
            var bidderEntity = new Bidders { UserId = bidderId, UserName = bidderName };

            _mockMediator.Setup(m => m.Send(It.IsAny<UpdateBidderCommand>(), default))
                         .ReturnsAsync(bidderEntity);

            var result = await _controller.UpdateBidder(bidderId, bidderDto) as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal(bidderEntity, result.Value);
        }

        [Fact]
        public async Task DeleteBidder_ReturnsBadRequest_WhenBidderIdIsEmpty()
        {
            var result = await _controller.DeleteBidder(Guid.Empty) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("El ID del postor no puede estar vacío.", result.Value);
        }

        [Fact]
        public async Task DeleteBidder_ReturnsOk_WhenBidderIsDeleted()
        {
            Guid bidderId = Guid.NewGuid();
           

            _mockMediator.Setup(m => m.Send(It.IsAny<DeleteBidderCommand>(), default))
                         .ReturnsAsync(bidderId);

            var result = await _controller.DeleteBidder(bidderId) as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal(bidderId, result.Value);
        }
        [Fact]
        public async Task CreateBidder_ReturnsInternalServerError_WhenExceptionOccurs()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<CreateBidderCommand>(), default))
                .ThrowsAsync(new Exception("Unexpected error"));

            var result = await _controller.CreateBidder(new CreateBidderDto()) as ObjectResult;

            Assert.NotNull(result);
            Assert.Equal(500, result.StatusCode);
            Assert.Equal("Error interno al crear el postor.", result.Value);
        }

        [Fact]
        public async Task GetBidders_ReturnsInternalServerError_WhenExceptionOccurs()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetBidderAllQuery>(), default))
                .ThrowsAsync(new Exception("Unexpected error"));

            var result = await _controller.GetBidders() as ObjectResult;

            Assert.NotNull(result);
            Assert.Equal(500, result.StatusCode);
            Assert.Equal("Error interno al obtener los postores.", result.Value);
        }

        [Fact]
        public async Task UpdateBidder_ReturnsInternalServerError_WhenExceptionOccurs()
        {
            Guid bidderId = Guid.NewGuid();
            var bidderDto = new UpdateBidderDto();

            _mockMediator.Setup(m => m.Send(It.IsAny<UpdateBidderCommand>(), default))
                .ThrowsAsync(new Exception("Unexpected error"));

            var result = await _controller.UpdateBidder(bidderId, bidderDto) as ObjectResult;

            Assert.NotNull(result);
            Assert.Equal(500, result.StatusCode);
            Assert.Equal("Error interno al actualizar el postor.", result.Value);
        }

        [Fact]
        public async Task DeleteBidder_ReturnsInternalServerError_WhenExceptionOccurs()
        {
            Guid bidderId = Guid.NewGuid();

            _mockMediator.Setup(m => m.Send(It.IsAny<DeleteBidderCommand>(), default))
                .ThrowsAsync(new Exception("Unexpected error"));

            var result = await _controller.DeleteBidder(bidderId) as ObjectResult;

            Assert.NotNull(result);
            Assert.Equal(500, result.StatusCode);
            Assert.Equal("Error interno al eliminar el postor.", result.Value);
        }

        [Fact]
        public async Task GetBidders_ReturnsOk_WithMultipleBidders()
        {
            var bidders = new List<GetBidderDto> { new GetBidderDto(), new GetBidderDto() };

            _mockMediator.Setup(m => m.Send(It.IsAny<GetBidderAllQuery>(), default))
                .ReturnsAsync(bidders);

            var result = await _controller.GetBidders() as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal(bidders, result.Value);
        }

        [Fact]
        public async Task GetBidderById_ReturnsInternalServerError_WhenExceptionOccurs()
        {
            Guid bidderId = Guid.NewGuid();

            _mockMediator.Setup(m => m.Send(It.IsAny<GetBidderByIdQuery>(), default))
                .ThrowsAsync(new Exception("Unexpected error"));

            var result = await _controller.GetBidderById(bidderId) as ObjectResult;

            Assert.NotNull(result);
            Assert.Equal(500, result.StatusCode);
            Assert.Equal("Error interno al buscar el postor.", result.Value);
        }

        [Fact]
        public async Task GetBidderByUserEmail_ReturnsBadRequest_WhenEmailIsInvalid()
        {
            var result = await _controller.GetBidderByUserEmail("invalid-email") as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("El correo electrónico no es válido.", result.Value);
        }

      

        [Fact]
        public async Task DeleteBidder_ReturnsNotFound_WhenBidderDoesNotExist()
        {
            Guid bidderId = Guid.NewGuid();
            _mockMediator.Setup(m => m.Send(It.IsAny<DeleteBidderCommand>(), default))
                .ReturnsAsync((Guid?)null);

            var result = await _controller.DeleteBidder(bidderId) as NotFoundObjectResult;

            Assert.NotNull(result);
            Assert.Equal(404, result.StatusCode);
            Assert.Equal($"No se encontró un postor con ID: {bidderId}", result.Value);
        }

       
    }
}
