using AutoMapper;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Application.Commands.Bidder;
using UserMs.Application.Handlers.Bidder.Command;
using UserMs.Commoon.Dtos.Users.Response.ActivityHistory;
using UserMs.Commoon.Dtos.Users.Response.Bidder;
using UserMs.Commoon.Dtos.Users.Response.User;
using UserMs.Core.RabbitMQ;
using UserMs.Core.Repositories.ActivityHistoryRepo;
using UserMs.Core.Repositories.Biddere;
using UserMs.Core.Repositories.Bidders;
using UserMs.Core.Repositories.UserRepo;
using UserMs.Core.Service.Keycloak;
using UserMs.Domain.Entities.Bidder;
using UserMs.Domain.Entities.UserEntity;
using UserMs.Domain.Entities;
using UserMs.Infrastructure.Exceptions;
using Xunit;

namespace UserMs.Test.Application.Handler.Command.Bidder
{
    public class DeleteBidderCommandHandlerTests
    {
        private readonly Mock<IBidderRepository> _bidderRepositoryMock;
        private readonly Mock<IBidderRepositoryMongo> _bidderRepositoryMongoMock;
        private readonly Mock<IEventBus<GetBidderDto>> _eventBusMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IKeycloakService> _keycloakServiceMock;
        private readonly Mock<IEventBus<GetUsersDto>> _eventBusUserMock;
        private readonly Mock<IUserRepository> _usersRepositoryMock;
        private readonly Mock<IUserRepositoryMongo> _usersRepositoryMongoMock;
        private readonly Mock<IActivityHistoryRepository> _activityHistoryRepositoryMock;
        private readonly Mock<IEventBus<GetActivityHistoryDto>> _eventBusActivityMock;
        private readonly DeleteBidderCommandHandler _handler;

        public DeleteBidderCommandHandlerTests()
        {
            _bidderRepositoryMock = new Mock<IBidderRepository>();
            _bidderRepositoryMongoMock = new Mock<IBidderRepositoryMongo>();
            _eventBusMock = new Mock<IEventBus<GetBidderDto>>();
            _mapperMock = new Mock<IMapper>();
            _keycloakServiceMock = new Mock<IKeycloakService>();
            _eventBusUserMock = new Mock<IEventBus<GetUsersDto>>();
            _usersRepositoryMock = new Mock<IUserRepository>();
            _usersRepositoryMongoMock = new Mock<IUserRepositoryMongo>();
            _activityHistoryRepositoryMock = new Mock<IActivityHistoryRepository>();
            _eventBusActivityMock = new Mock<IEventBus<GetActivityHistoryDto>>();

            _handler = new DeleteBidderCommandHandler(
                _eventBusActivityMock.Object,
                _activityHistoryRepositoryMock.Object,
                _bidderRepositoryMock.Object,
                _bidderRepositoryMongoMock.Object,
                _usersRepositoryMock.Object,
                _usersRepositoryMongoMock.Object,
                _keycloakServiceMock.Object,
                _eventBusUserMock.Object,
                _eventBusMock.Object,
                _mapperMock.Object
            );
        }

        /// <summary>
        /// Verifica que cuando el postor existe, se elimina correctamente.
        /// </summary>
        [Fact]
        public async Task Handle_ShouldDeleteBidder_WhenBidderExists()
        {
            // Arrange
            var bidderId = Guid.NewGuid();
            var bidder = new Bidders { UserId = UserId.Create(bidderId) };
            var users = new Users { UserId = bidderId };

            _bidderRepositoryMongoMock
                .Setup(repo => repo.GetBidderByIdAsync(It.IsAny<UserId>()))
                .ReturnsAsync(bidder);

            _usersRepositoryMongoMock
                .Setup(repo => repo.GetUsersById(It.IsAny<Guid>()))
                .ReturnsAsync(users);

            _bidderRepositoryMock
                .Setup(repo => repo.DeleteAsync(It.IsAny<UserId>()))
                .ReturnsAsync(bidder);

            _keycloakServiceMock
                .Setup(service => service.DeleteUserAsync(It.IsAny<Guid>()))
                .ReturnsAsync("mocked-deletion-result");

            _usersRepositoryMock
                .Setup(repo => repo.DeleteUsersAsync(It.IsAny<UserId>()))
                .ReturnsAsync(users);

            // Act
            var result = await _handler.Handle(new DeleteBidderCommand(UserId.Create(bidderId)), CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(bidderId, result.Value);
        }

        /// <summary>
        /// Verifica que cuando el postor no existe, se lanza `UserNotFoundException`.
        /// </summary>
        [Fact]
        public async Task Handle_ShouldThrowUserNotFoundException_WhenBidderDoesNotExist()
        {
            // Arrange
            var bidderId = Guid.NewGuid();

            _bidderRepositoryMongoMock
                .Setup(repo => repo.GetBidderByIdAsync(It.IsAny<UserId>()))
                .ReturnsAsync((Bidders)null); // Simula que el postor no existe

            // Act & Assert
            await Assert.ThrowsAsync<UserNotFoundException>(() => _handler.Handle(new DeleteBidderCommand(bidderId), CancellationToken.None));
        }

        /// <summary>
        /// Verifica que cuando ocurre un error inesperado, la excepción se propaga correctamente.
        /// </summary>
        [Fact]
        public async Task Handle_ShouldThrowException_WhenUnexpectedErrorOccurs()
        {
            // Arrange
            var bidderId = Guid.NewGuid();

            _bidderRepositoryMongoMock
                .Setup(repo => repo.GetBidderByIdAsync(It.IsAny<UserId>()))
                .ThrowsAsync(new Exception("Unexpected error"));

            // Act & Assert
            await Assert.ThrowsAsync<ApplicationException>(() => _handler.Handle(new DeleteBidderCommand(bidderId), CancellationToken.None));
        }
    }

}
