using AutoMapper;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Application.Handlers.Bidder.Queries;
using UserMs.Application.Queries.Bidder;
using UserMs.Commoon.Dtos.Users.Response.ActivityHistory;
using UserMs.Commoon.Dtos.Users.Response.Bidder;
using UserMs.Core.RabbitMQ;
using UserMs.Core.Repositories.ActivityHistoryRepo;
using UserMs.Core.Repositories.Bidders;
using UserMs.Domain.Entities.Bidder;
using UserMs.Domain.Entities.IUser.ValueObjects;
using Xunit;

namespace UserMs.Test.Application.Handler.Queries.Bidder
{
    public class GetBidderAllQueryHandlerTests
    {
        private readonly Mock<IBidderRepositoryMongo> _bidderRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IActivityHistoryRepository> _activityHistoryRepositoryMock;
        private readonly Mock<IEventBus<GetActivityHistoryDto>> _eventBusActivityMock;
        private readonly GetBidderAllQueryHandler _handler;

        public GetBidderAllQueryHandlerTests()
        {
            _bidderRepositoryMock = new Mock<IBidderRepositoryMongo>();
            _mapperMock = new Mock<IMapper>();
            _activityHistoryRepositoryMock = new Mock<IActivityHistoryRepository>();
            _eventBusActivityMock = new Mock<IEventBus<GetActivityHistoryDto>>();

            _handler = new GetBidderAllQueryHandler(
                _eventBusActivityMock.Object,
                _activityHistoryRepositoryMock.Object,
                _bidderRepositoryMock.Object,
                _mapperMock.Object
            );
        }

        /// <summary>
        /// Verifica que cuando hay postores, se devuelvan correctamente los datos mapeados.
        /// </summary>
        [Fact]
        public async Task Handle_ShouldReturnMappedBidders_WhenBiddersExist()
        {
            // Arrange
            var name1 = UserName.Create("Postor 1");
            var name2 = UserName.Create("Postor 2");
            var bidders = new List<Bidders>
        {
            new Bidders { UserId = Guid.NewGuid(), UserName = name1 },
            new Bidders { UserId = Guid.NewGuid(), UserName = name2 }
        };
            var expectedDtos = new List<GetBidderDto>
        {
            new GetBidderDto { UserId = bidders[0].UserId, UserName = bidders[0].UserName.Value },
            new GetBidderDto { UserId = bidders[1].UserId, UserName = bidders[1].UserName.Value }
        };

            _bidderRepositoryMock
                .Setup(repo => repo.GetBidderAllAsync())
                .ReturnsAsync(bidders);

            _mapperMock
                .Setup(mapper => mapper.Map<List<GetBidderDto>>(bidders))
                .Returns(expectedDtos);

            // Act
            var result = await _handler.Handle(new GetBidderAllQuery(), CancellationToken.None);

            // Assert
            Assert.Equal(expectedDtos.Count, result.Count);
            Assert.Equal(expectedDtos[0].UserName, result[0].UserName);
            Assert.Equal(expectedDtos[1].UserName, result[1].UserName);
        }

        /// <summary>
        /// Verifica que cuando no hay postores, se retorne una lista vacía.
        /// </summary>
        [Fact]
        public async Task Handle_ShouldReturnEmptyList_WhenNoBiddersFound()
        {
            // Arrange
            _bidderRepositoryMock
                .Setup(repo => repo.GetBidderAllAsync())
                .ReturnsAsync(new List<Bidders>());

            // Act
            var result = await _handler.Handle(new GetBidderAllQuery(), CancellationToken.None);

            // Assert
            Assert.Empty(result);
        }

        /// <summary>
        /// Verifica que cuando ocurre una excepción en el repositorio, se propaga correctamente.
        /// </summary>
        [Fact]
        public async Task Handle_ShouldThrowException_WhenRepositoryFails()
        {
            // Arrange
            _bidderRepositoryMock
                .Setup(repo => repo.GetBidderAllAsync())
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _handler.Handle(new GetBidderAllQuery(), CancellationToken.None));
        }
    }

}
