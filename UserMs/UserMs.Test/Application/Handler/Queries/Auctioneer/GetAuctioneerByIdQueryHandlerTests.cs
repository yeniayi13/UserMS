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
using UserMs.Domain.Entities;
using UserMs.Domain.Entities.Auctioneer;
using UserMs.Domain.Entities.IUser.ValueObjects;
using UserMs.Infrastructure.Exceptions;
using Xunit;

namespace UserMs.Test.Application.Handler.Queries.Auctioneer
{
    public class GetAuctioneerByIdQueryHandlerTests
    {
        private readonly Mock<IAuctioneerRepositoryMongo> _auctioneerRepositoryMock;
        private readonly Mock<IActivityHistoryRepository> _activityHistoryRepositoryMock;
        private readonly Mock<IEventBus<GetActivityHistoryDto>> _eventBusActivityMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly GetAuctioneerByIdQueryHandler _handler;

        public GetAuctioneerByIdQueryHandlerTests()
        {
            _auctioneerRepositoryMock = new Mock<IAuctioneerRepositoryMongo>();
            _activityHistoryRepositoryMock = new Mock<IActivityHistoryRepository>();
            _eventBusActivityMock = new Mock<IEventBus<GetActivityHistoryDto>>();
            _mapperMock = new Mock<IMapper>();

            _handler = new GetAuctioneerByIdQueryHandler(
                _eventBusActivityMock.Object,
                _activityHistoryRepositoryMock.Object,
                _auctioneerRepositoryMock.Object,
                _mapperMock.Object
            );
        }

        /// <summary>
        /// Verifica que cuando el subastador existe, se devuelve correctamente el DTO mapeado.
        /// </summary>
        [Fact]
        public async Task Handle_ShouldReturnMappedAuctioneer_WhenAuctioneerExists()
        {
            // Arrange
            var auctioneerId = Guid.NewGuid();
            var auctioneerName = UserName.Create("Subastador 1");
            var auctioneer = new Auctioneers { UserId = auctioneerId, UserName = auctioneerName };
            var expectedDto = new GetAuctioneerDto { UserId = auctioneerId, UserName = auctioneerName.Value };
            var activityHistory = new Domain.Entities.ActivityHistory.ActivityHistory(Guid.NewGuid(), auctioneerId, "Busco Subastador por Id", DateTime.UtcNow);
            var activityDto = new GetActivityHistoryDto();

            _auctioneerRepositoryMock
                .Setup(repo => repo.GetAuctioneerByIdAsync(It.IsAny<UserId>()))
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
            var result = await _handler.Handle(new GetAuctioneerByIdQuery(auctioneerId), CancellationToken.None);

            // Assert
            Assert.Equal(expectedDto.UserId, result.UserId);
            Assert.Equal(expectedDto.UserName, result.UserName);
        }

       
        [Fact] 
        public async Task Handle_ShouldThrowUserNotFoundException_WhenAuctioneerNotFound()
        {
            // Arrange
            var auctioneerId = Guid.NewGuid();

            _auctioneerRepositoryMock
                .Setup(repo => repo.GetAuctioneerByIdAsync(It.IsAny<UserId>()))
                .ReturnsAsync((Auctioneers)null); // Simulamos que el repositorio devuelve `null`

            // Act & Assert
            await Assert.ThrowsAsync<UserNotFoundException>(() => _handler.Handle(new GetAuctioneerByIdQuery(auctioneerId), CancellationToken.None));
        }


        [Fact]
        public async Task Handle_ShouldThrowException_WhenRepositoryFails()
        {
            // Arrange
            var auctioneerId = Guid.NewGuid();

            _auctioneerRepositoryMock
                .Setup(repo => repo.GetAuctioneerByIdAsync(It.IsAny<UserId>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _handler.Handle(new GetAuctioneerByIdQuery(auctioneerId), CancellationToken.None));
        }
    }

}
