using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using AutoMapper;
using UserMs.Application.Handlers.ActivityHistory.Queries;
using UserMs.Application.Queries.HistoryActivity;
using UserMs.Commoon.Dtos.Users.Response.ActivityHistory;
using UserMs.Core.Repositories.ActivityHistory;
using UserMs.Core.Repositories.ActivityHistoryRepo;
using UserMs.Domain.Entities;
namespace UserMs.Test.Application.Handler.Queries.Activity
{
   

    public class GetActivitiesByUserQueryHandlerTests
    {
        private readonly Mock<IActivityHistoryRepositoryMongo> _activityHistoryRepositoryMongoMock;
        private readonly Mock<IActivityHistoryRepository> _activityHistoryRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILogger<GetActivitiesByUserQueryHandler>> _loggerMock;
        private readonly GetActivitiesByUserQueryHandler _handler;

        public GetActivitiesByUserQueryHandlerTests()
        {
            _activityHistoryRepositoryMongoMock = new Mock<IActivityHistoryRepositoryMongo>();
            _activityHistoryRepositoryMock = new Mock<IActivityHistoryRepository>();
            _mapperMock = new Mock<IMapper>();
            _loggerMock = new Mock<ILogger<GetActivitiesByUserQueryHandler>>();

            _handler = new GetActivitiesByUserQueryHandler(
                _activityHistoryRepositoryMongoMock.Object,
                _mapperMock.Object,
                _loggerMock.Object,
                _activityHistoryRepositoryMock.Object
            );
        }

        [Fact]
        public async Task Handle_ShouldReturnMappedActivities_WhenActivitiesExist()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var request = new GetActivitiesByUserQuery(userId, DateTime.UtcNow.AddDays(-7), DateTime.UtcNow);
            var activities = new List<Domain.Entities.ActivityHistory.ActivityHistory>
        {
            new Domain.Entities.ActivityHistory.ActivityHistory(Guid.NewGuid(), userId, "ACTION_1", DateTime.UtcNow),
            new Domain.Entities.ActivityHistory.ActivityHistory(Guid.NewGuid(), userId, "ACTION_2", DateTime.UtcNow)
        };
            var expectedDtos = new List<GetActivityHistoryDto>
        {
            new GetActivityHistoryDto { UserId = userId, Action = "ACTION_1" },
            new GetActivityHistoryDto { UserId = userId, Action = "ACTION_2" }
        };

            _activityHistoryRepositoryMongoMock
                .Setup(repo => repo.GetActivitiesByUserAsync(It.IsAny<UserId>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(activities);

            _mapperMock
                .Setup(mapper => mapper.Map<List<GetActivityHistoryDto>>(activities))
                .Returns(expectedDtos);

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.Equal(expectedDtos.Count, result.Count);
            Assert.Equal(expectedDtos[0].Action, result[0].Action);
            Assert.Equal(expectedDtos[1].Action, result[1].Action);
        }

        [Fact]
        public async Task Handle_ShouldLogWarningAndReturnEmptyList_WhenNoActivitiesFound()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var request = new GetActivitiesByUserQuery(userId, DateTime.UtcNow.AddDays(-7), DateTime.UtcNow);

            _activityHistoryRepositoryMongoMock
                .Setup(repo => repo.GetActivitiesByUserAsync(It.IsAny<UserId>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(new List<Domain.Entities.ActivityHistory.ActivityHistory>());

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.Empty(result);
            _activityHistoryRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Domain.Entities.ActivityHistory.ActivityHistory>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldCatchExceptionAndReturnEmptyList_WhenExceptionOccurs()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var request = new GetActivitiesByUserQuery(userId, DateTime.UtcNow.AddDays(-7), DateTime.UtcNow);

            _activityHistoryRepositoryMongoMock
                .Setup(repo => repo.GetActivitiesByUserAsync(It.IsAny<UserId>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.Empty(result);
            _loggerMock.Verify(logger =>
                    logger.Log(
                        It.Is<LogLevel>(logLevel => logLevel == LogLevel.Error),
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((object v, Type _) => v.ToString().Contains("Database error")),
                        It.IsAny<Exception>(),
                        It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
