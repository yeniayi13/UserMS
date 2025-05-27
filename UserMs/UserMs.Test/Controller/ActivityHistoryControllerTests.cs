using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using UserMs.Application.Commands.ActivityHistory;
using UserMs.Application.Queries.HistoryActivity;
using UserMs.Commoon.Dtos.Users.Request.ActivityHistory;
using UserMs.Controllers;
using Xunit;
using UserMs.Commoon.Dtos.Users.Response.ActivityHistory;
using UserMs.Domain.Entities;

namespace UserMs.Test.Controller
{
   

    public class ActivityHistoryControllerTests
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly Mock<ILogger<ActivityHistoryController>> _mockLogger;
        private readonly ActivityHistoryController _controller;

        public ActivityHistoryControllerTests()
        {
            _mockMediator = new Mock<IMediator>();
            _mockLogger = new Mock<ILogger<ActivityHistoryController>>();
            _controller = new ActivityHistoryController(_mockMediator.Object, _mockLogger.Object);
        }

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenMediatorIsNull()
        {
            var exception = Assert.Throws<ArgumentNullException>(() => new ActivityHistoryController(null, _mockLogger.Object));

            Assert.Equal("mediator", exception.ParamName);
        }

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenLoggerIsNull()
        {
            var exception = Assert.Throws<ArgumentNullException>(() => new ActivityHistoryController(_mockMediator.Object, null));

            Assert.Equal("logger", exception.ParamName);
        }

        [Fact]
        public async Task GetActivitiesByUserId_ReturnsBadRequest_WhenUserIdIsEmpty()
        {
            var result = await _controller.GetActivitiesByUserId(Guid.Empty, null, null) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("El ID del usuario no puede estar vacío.", result.Value);
        }

        [Fact]
        public async Task GetActivitiesByUserId_ReturnsNotFound_WhenNoActivitiesExist()
        {
            Guid userId = Guid.NewGuid();
            _mockMediator.Setup(m => m.Send(It.IsAny<GetActivitiesByUserQuery>(), default))
                         .ReturnsAsync((List<GetActivityHistoryDto>)null);

            var result = await _controller.GetActivitiesByUserId(userId, null, null) as NotFoundObjectResult;

            Assert.NotNull(result);
            Assert.Equal(404, result.StatusCode);
            Assert.Equal($"No se encontraron actividades para el usuario con ID: {userId}", result.Value);
        }

        [Fact]
        public async Task GetActivitiesByUserId_ReturnsOk_WhenActivitiesExist()
        {
            Guid userId = Guid.NewGuid();
            var mockActivities = new List<GetActivityHistoryDto> { new GetActivityHistoryDto() };

            _mockMediator.Setup(m => m.Send(It.IsAny<GetActivitiesByUserQuery>(), default))
                         .ReturnsAsync(mockActivities);

            var result = await _controller.GetActivitiesByUserId(userId, null, null) as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal(mockActivities, result.Value);
        }

        [Fact]
        public async Task CreateActivityHistory_ReturnsBadRequest_WhenUserIdIsEmpty()
        {
            var result = await _controller.CreateActivityHistory(Guid.Empty, new CreateActivityHistoryDto()) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("El ID del usuario no puede estar vacío.", result.Value);
        }

        [Fact]
        public async Task CreateActivityHistory_ReturnsBadRequest_WhenDtoIsNull()
        {
            var result = await _controller.CreateActivityHistory(Guid.NewGuid(), null) as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("La actividad no puede estar vacía.", result.Value);
        }

        [Fact]
        public async Task CreateActivityHistory_ReturnsOk_WhenActivityIsCreated()
        {
            UserId userId = Guid.NewGuid();
            var historyDto = new CreateActivityHistoryDto();
            var createdActivityUserId = userId;

            _mockMediator.Setup(m => m.Send(It.IsAny<CreateHistoryActivityCommand>(), default))
                         .ReturnsAsync(createdActivityUserId);

            var result = await _controller.CreateActivityHistory(userId, historyDto) as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
           
        }

        [Fact]
        public async Task GetActivitiesByUserId_ReturnsInternalServerError_WhenExceptionOccurs()
        {
            Guid userId = Guid.NewGuid();

            _mockMediator.Setup(m => m.Send(It.IsAny<GetActivitiesByUserQuery>(), default))
                .ThrowsAsync(new Exception("Unexpected error"));

            var result = await _controller.GetActivitiesByUserId(userId, null, null) as ObjectResult;

            Assert.NotNull(result);
            Assert.Equal(500, result.StatusCode);
            Assert.Equal("Error interno al obtener el historial de actividades.", result.Value);
        }

        [Fact]
        public async Task CreateActivityHistory_ReturnsInternalServerError_WhenExceptionOccurs()
        {
            Guid userId = Guid.NewGuid();
            var historyDto = new CreateActivityHistoryDto();

            _mockMediator.Setup(m => m.Send(It.IsAny<CreateHistoryActivityCommand>(), default))
                .ThrowsAsync(new Exception("Unexpected error"));

            var result = await _controller.CreateActivityHistory(userId, historyDto) as ObjectResult;

            Assert.NotNull(result);
            Assert.Equal(500, result.StatusCode);
            Assert.Equal("Error interno al registrar actividad.", result.Value);
        }

        [Fact]
        public async Task GetActivitiesByUserId_ReturnsOk_WithDateRangeFilters()
        {
            Guid userId = Guid.NewGuid();
            var startDate = DateTime.UtcNow.AddDays(-7);
            var endDate = DateTime.UtcNow;
            var mockActivities = new List<GetActivityHistoryDto> { new GetActivityHistoryDto() };

            _mockMediator.Setup(m => m.Send(It.Is<GetActivitiesByUserQuery>(
                    q => q.StartDate == startDate && q.EndDate == endDate), default))
                .ReturnsAsync(mockActivities);

            var result = await _controller.GetActivitiesByUserId(userId, startDate, endDate) as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.Equal(mockActivities, result.Value);
        }
    }
}
