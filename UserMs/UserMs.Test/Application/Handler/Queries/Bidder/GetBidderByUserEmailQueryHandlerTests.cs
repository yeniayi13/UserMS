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
using UserMs.Domain.Entities;
using UserMs.Domain.Entities.Bidder;
using UserMs.Infrastructure.Exceptions;
using Xunit;

namespace UserMs.Test.Application.Handler.Queries.Bidder
{
    public class GetBidderByUserEmailQueryHandlerTests
    {
        private readonly Mock<IBidderRepositoryMongo> _bidderRepositoryMock;
        private readonly Mock<IActivityHistoryRepository> _activityHistoryRepositoryMock;
        private readonly Mock<IEventBus<GetActivityHistoryDto>> _eventBusActivityMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly GetBidderByUserEmailQueryHandler _handler;

        public GetBidderByUserEmailQueryHandlerTests()
        {
            _bidderRepositoryMock = new Mock<IBidderRepositoryMongo>();
            _activityHistoryRepositoryMock = new Mock<IActivityHistoryRepository>();
            _eventBusActivityMock = new Mock<IEventBus<GetActivityHistoryDto>>();
            _mapperMock = new Mock<IMapper>();

            _handler = new GetBidderByUserEmailQueryHandler(
                _eventBusActivityMock.Object,
                _activityHistoryRepositoryMock.Object,
                _bidderRepositoryMock.Object,
                _mapperMock.Object
            );
        }

        /// <summary>
        /// Verifica que cuando el postor existe, se devuelve correctamente el DTO mapeado.
        /// </summary>
        [Fact]
        public async Task Handle_ShouldReturnMappedBidder_WhenBidderExists()
        {
            // Arrange
            var email = UserEmail.Create("postor1@example.com");
            var bidder = new Bidders { UserId = Guid.NewGuid(), UserEmail = email };
            var expectedDto = new GetBidderDto { UserId = bidder.UserId, UserEmail = email.Value };
            var activityHistory = new Domain.Entities.ActivityHistory.ActivityHistory(Guid.NewGuid(), bidder.UserId, "Busco Postores", DateTime.UtcNow);
            var activityDto = new GetActivityHistoryDto();

            _bidderRepositoryMock
                .Setup(repo => repo.GetBidderByEmailAsync(email))
                .ReturnsAsync(bidder);

            _mapperMock
                .Setup(mapper => mapper.Map<GetBidderDto>(bidder))
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
            var result = await _handler.Handle(new GetBidderByUserEmailQuery(email.Value), CancellationToken.None);

            // Assert
            Assert.Equal(expectedDto.UserId, result.UserId);
            Assert.Equal(expectedDto.UserEmail, result.UserEmail);
        }

        /// <summary>
        /// Verifica que cuando el postor no se encuentra, se retorne `null`.
        /// </summary>
        [Fact]
        public async Task Handle_ShouldThrowUserNotFoundException_WhenBidderNotFound()
        {
            // Arrange
            var email = UserEmail.Create("notfound@example.com");

            _bidderRepositoryMock
                .Setup(repo => repo.GetBidderByEmailAsync(email))
                .ReturnsAsync((Bidders)null); // Simulamos que el repositorio devuelve `null`

            // Act & Assert
            await Assert.ThrowsAsync<UserNotFoundException>(() => _handler.Handle(new GetBidderByUserEmailQuery(email.Value), CancellationToken.None));
        }
        /// <summary>
        /// Verifica que cuando ocurre una excepción, se maneja correctamente y se retorna `null`.
        /// </summary>
        [Fact]
        public async Task Handle_ShouldThrowException_WhenRepositoryFails()
        {
            // Arrange
            var email = UserEmail.Create("error@example.com");

            _bidderRepositoryMock
                .Setup(repo => repo.GetBidderByEmailAsync(email))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _handler.Handle(new GetBidderByUserEmailQuery(email.Value), CancellationToken.None));
        }
    }

}
