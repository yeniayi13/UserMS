using AutoMapper;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Application.Handlers.Auctioneer.Queries;
using UserMs.Application.Queries.Auctioneer;
using UserMs.Commoon.Dtos.Users.Response.ActivityHistory;
using UserMs.Commoon.Dtos.Users.Response.Auctioneer;
using UserMs.Core.RabbitMQ;
using UserMs.Core.Repositories.ActivityHistoryRepo;
using UserMs.Core.Repositories.Auctioneer;
using UserMs.Domain.Entities.Auctioneer;
using UserMs.Domain.Entities;
using Xunit;
using UserMs.Infrastructure.Exceptions;

namespace UserMs.Test.Application.Handler.Queries.Auctioneer
{
    public class GetAuctioneerByUserEmailQueryHandlerTests
    {
        private readonly Mock<IAuctioneerRepositoryMongo> _auctioneerRepositoryMock;
        private readonly Mock<IActivityHistoryRepository> _activityHistoryRepositoryMock;
        private readonly Mock<IEventBus<GetActivityHistoryDto>> _eventBusActivityMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly GetAuctioneerByUserEmailQueryHandler _handler;

        public GetAuctioneerByUserEmailQueryHandlerTests()
        {
            _auctioneerRepositoryMock = new Mock<IAuctioneerRepositoryMongo>();
            _activityHistoryRepositoryMock = new Mock<IActivityHistoryRepository>();
            _eventBusActivityMock = new Mock<IEventBus<GetActivityHistoryDto>>();
            _mapperMock = new Mock<IMapper>();

            _handler = new GetAuctioneerByUserEmailQueryHandler(
                _eventBusActivityMock.Object,
                _activityHistoryRepositoryMock.Object,
                _auctioneerRepositoryMock.Object,
                _mapperMock.Object
            );
        }

        
        [Fact]
        public async Task Handle_ShouldReturnMappedAuctioneer_WhenAuctioneerExists()
        {
            // Arrange
            var email = UserEmail.Create("subastador1@example.com");
            var auctioneer = new Auctioneers { UserId = Guid.NewGuid(), UserEmail = email };
            var expectedDto = new GetAuctioneerDto { UserId = auctioneer.UserId, UserEmail = email.Value };
            var activityHistory = new Domain.Entities.ActivityHistory.ActivityHistory(Guid.NewGuid(), auctioneer.UserId, "Busco Subastador por email", DateTime.UtcNow);
            var activityDto = new GetActivityHistoryDto();

            _auctioneerRepositoryMock
                .Setup(repo => repo.GetAuctioneerByEmailAsync(email))
                .ReturnsAsync(auctioneer);

            _mapperMock
                .Setup(mapper => mapper.Map<GetAuctioneerDto>(auctioneer))
                .Returns(expectedDto);

            _activityHistoryRepositoryMock
                .Setup(repo => repo.AddAsync(It.IsAny<Domain.Entities.ActivityHistory.ActivityHistory>()))
                .Returns(Task.CompletedTask);

            _mapperMock
                .Setup(mapper => mapper.Map<GetActivityHistoryDto>(It.IsAny<Domain.Entities.ActivityHistory.ActivityHistory>()))
                .Returns(activityDto);

            _eventBusActivityMock
                .Setup(bus => bus.PublishMessageAsync(activityDto, "activityHistoryQueue", "ACTIVITY_CREATED"))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(new GetAuctioneerByUserEmailQuery(email.Value), CancellationToken.None);

            // Assert
            Assert.Equal(expectedDto.UserId, result.UserId);
            Assert.Equal(expectedDto.UserEmail, result.UserEmail);
        }


        [Fact]
        public async Task Handle_ShouldThrowUserNotFoundException_WhenAuctioneerNotFound()
        {
            // Arrange
            var email = UserEmail.Create("notfound@example.com");

            _auctioneerRepositoryMock
                .Setup(repo => repo.GetAuctioneerByEmailAsync(email))
                .ReturnsAsync((Auctioneers)null); // Simulamos que el repositorio devuelve `null`

            // Act & Assert
            await Assert.ThrowsAsync<UserNotFoundException>(() => _handler.Handle(new GetAuctioneerByUserEmailQuery(email.Value), CancellationToken.None));
        }


        [Fact]
        public async Task Handle_ShouldThrowException_WhenRepositoryFails()
        {
            // Arrange
            var email = UserEmail.Create("error@example.com");

            _auctioneerRepositoryMock
                .Setup(repo => repo.GetAuctioneerByEmailAsync(email))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _handler.Handle(new GetAuctioneerByUserEmailQuery(email.Value), CancellationToken.None));
        }
    }

}
