using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using Xunit;
using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using UserMs.Application.Commands.ActivityHistory;
using UserMs.Application.Handlers.ActivityHistory.Command;
using UserMs.Application.Handlers.ActivityHistory;
using UserMs.Commoon.Dtos.Users.Response.ActivityHistory;
using UserMs.Core.RabbitMQ;
using UserMs.Core.Repositories.ActivityHistoryRepo;
using UserMs.Domain.Entities;
using UserMs.Commoon.Dtos.Users.Request.ActivityHistory;

namespace UserMs.Test.Application.Handler.Command.Activity
{
    

    public class CreateHistoryActivityCommandHandlerTests
    {
        private readonly Mock<IActivityHistoryRepository> _activityHistoryRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IEventBus<GetActivityHistoryDto>> _eventBusMock;
        private readonly CreateHistoryActivityCommandHandler _handler;

        public CreateHistoryActivityCommandHandlerTests()
        {
            _activityHistoryRepositoryMock = new Mock<IActivityHistoryRepository>();
            _mapperMock = new Mock<IMapper>();
            _eventBusMock = new Mock<IEventBus<GetActivityHistoryDto>>();

            _handler = new CreateHistoryActivityCommandHandler(
                _activityHistoryRepositoryMock.Object,
                _mapperMock.Object,
                _eventBusMock.Object
            );
        }

        [Fact]
        public async Task Handle_ShouldRegisterActivityAndReturnUserId()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var request = new CreateHistoryActivityCommand(new CreateActivityHistoryDto { UserId = userId, Action = "TEST_ACTION" });
            var activityHistory = new Domain.Entities.ActivityHistory.ActivityHistory(Guid.NewGuid(), userId, "TEST_ACTION", DateTime.UtcNow);
            var activityDto = new GetActivityHistoryDto();

            _activityHistoryRepositoryMock
                .Setup(repo => repo.AddAsync(It.IsAny<Domain.Entities.ActivityHistory.ActivityHistory>()))
                .Returns(Task.CompletedTask);

            _mapperMock
                .Setup(mapper => mapper.Map<GetActivityHistoryDto>(It.IsAny<Domain.Entities.ActivityHistory.ActivityHistory>()))
                .Returns(activityDto);

            _eventBusMock
                .Setup(bus => bus.PublishMessageAsync(activityDto, "activityQueue", "ACTIVITY_CREATED"))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.Equal(userId, result.Value);
            _activityHistoryRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Domain.Entities.ActivityHistory.ActivityHistory>()), Times.Once);
            _mapperMock.Verify(mapper => mapper.Map<GetActivityHistoryDto>(It.IsAny<Domain.Entities.ActivityHistory.ActivityHistory>()), Times.Once);
            _eventBusMock.Verify(bus => bus.PublishMessageAsync(activityDto, "activityQueue", "ACTIVITY_CREATED"), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldThrowException_WhenRepositoryFails()
        {
            // Arrange
            var request = new CreateHistoryActivityCommand(new CreateActivityHistoryDto{ UserId =Guid.NewGuid(), Action = "TEST_ACTION" });

            _activityHistoryRepositoryMock
                .Setup(repo => repo.AddAsync(It.IsAny<Domain.Entities.ActivityHistory.ActivityHistory>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _handler.Handle(request, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_ShouldPublishCorrectMessageToEventBus()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var request = new CreateHistoryActivityCommand(new CreateActivityHistoryDto { UserId = userId, Action = "TEST_ACTION" });
            var activityHistory = new Domain.Entities.ActivityHistory.ActivityHistory(Guid.NewGuid(), userId, "TEST_ACTION", DateTime.UtcNow);
            var activityDto = new GetActivityHistoryDto { UserId = userId, Action = "TEST_ACTION" };

            _activityHistoryRepositoryMock
                .Setup(repo => repo.AddAsync(It.IsAny<Domain.Entities.ActivityHistory.ActivityHistory>()))
                .Returns(Task.CompletedTask);

            _mapperMock
                .Setup(mapper => mapper.Map<GetActivityHistoryDto>(It.IsAny<Domain.Entities.ActivityHistory.ActivityHistory>()))
                .Returns(activityDto);

            _eventBusMock
                .Setup(bus => bus.PublishMessageAsync(activityDto, "activityQueue", "ACTIVITY_CREATED"))
                .Returns(Task.CompletedTask);

            // Act
            await _handler.Handle(request, CancellationToken.None);

            // Assert
            _eventBusMock.Verify(bus => bus.PublishMessageAsync(
                It.Is<GetActivityHistoryDto>(dto => dto.UserId == userId && dto.Action == "TEST_ACTION"),
                "activityQueue",
                "ACTIVITY_CREATED"), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldThrowException_WhenMapperFails()
        {
            // Arrange
            var request = new CreateHistoryActivityCommand(new CreateActivityHistoryDto { UserId = Guid.NewGuid(), Action = "TEST_ACTION" });

            _activityHistoryRepositoryMock
                .Setup(repo => repo.AddAsync(It.IsAny<Domain.Entities.ActivityHistory.ActivityHistory>()))
                .Returns(Task.CompletedTask);

            _mapperMock
                .Setup(mapper => mapper.Map<GetActivityHistoryDto>(It.IsAny<Domain.Entities.ActivityHistory.ActivityHistory>()))
                .Throws(new Exception("Mapping error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _handler.Handle(request, CancellationToken.None));
        }

      /*  [Fact]
        public async Task Handle_ShouldRespectCancellationToken()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var request = new CreateHistoryActivityCommand(new CreateActivityHistoryDto { UserId = userId, Action = "TEST_ACTION" });
            var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;

            cancellationTokenSource.Cancel(); // Simulamos cancelación antes de ejecutar el método.

            // Act & Assert
            await Assert.ThrowsAsync<TaskCanceledException>(() => _handler.Handle(request, cancellationToken));
        }*/
    }
}
