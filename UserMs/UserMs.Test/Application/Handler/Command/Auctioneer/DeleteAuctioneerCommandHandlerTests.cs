using AutoMapper;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Application.Commands.Auctioneer;
using UserMs.Application.Handlers.Auctioneer.Command;
using UserMs.Commoon.Dtos.Users.Response.ActivityHistory;
using UserMs.Commoon.Dtos.Users.Response.Auctioneer;
using UserMs.Commoon.Dtos.Users.Response.User;
using UserMs.Core.RabbitMQ;
using UserMs.Core.Repositories.ActivityHistoryRepo;
using UserMs.Core.Repositories.Auctioneer;
using UserMs.Core.Repositories.UserRepo;
using UserMs.Core.Service.Keycloak;
using UserMs.Domain.Entities.Auctioneer;
using UserMs.Domain.Entities.UserEntity;
using UserMs.Domain.Entities;
using UserMs.Infrastructure.Exceptions;
using Xunit;

namespace UserMs.Test.Application.Handler.Command.Auctioneer
{
    public class DeleteAuctioneerCommandHandlerTests
    {
        private readonly Mock<IAuctioneerRepository> _auctioneerRepositoryMock;
        private readonly Mock<IAuctioneerRepositoryMongo> _auctioneerRepositoryMongoMock;
        private readonly Mock<IEventBus<GetAuctioneerDto>> _eventBusMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IKeycloakService> _keycloakServiceMock;
        private readonly Mock<IEventBus<GetUsersDto>> _eventBusUserMock;
        private readonly Mock<IUserRepository> _usersRepositoryMock;
        private readonly Mock<IUserRepositoryMongo> _usersRepositoryMongoMock;
        private readonly Mock<IActivityHistoryRepository> _activityHistoryRepositoryMock;
        private readonly Mock<IEventBus<GetActivityHistoryDto>> _eventBusActivityMock;
        private readonly DeleteAuctioneerCommandHandler _handler;

        public DeleteAuctioneerCommandHandlerTests()
        {
            _auctioneerRepositoryMock = new Mock<IAuctioneerRepository>();
            _auctioneerRepositoryMongoMock = new Mock<IAuctioneerRepositoryMongo>();
            _eventBusMock = new Mock<IEventBus<GetAuctioneerDto>>();
            _mapperMock = new Mock<IMapper>();
            _keycloakServiceMock = new Mock<IKeycloakService>();
            _eventBusUserMock = new Mock<IEventBus<GetUsersDto>>();
            _usersRepositoryMock = new Mock<IUserRepository>();
            _usersRepositoryMongoMock = new Mock<IUserRepositoryMongo>();
            _activityHistoryRepositoryMock = new Mock<IActivityHistoryRepository>();
            _eventBusActivityMock = new Mock<IEventBus<GetActivityHistoryDto>>();

            _handler = new DeleteAuctioneerCommandHandler(
                _eventBusActivityMock.Object,
                _activityHistoryRepositoryMock.Object,
                _auctioneerRepositoryMock.Object,
                _auctioneerRepositoryMongoMock.Object,
                _usersRepositoryMock.Object,
                _usersRepositoryMongoMock.Object,
                _keycloakServiceMock.Object,
                _eventBusUserMock.Object,
                _eventBusMock.Object,
                _mapperMock.Object
            );
        }

        /// <summary>
        /// Verifica que cuando el subastador existe, se elimina correctamente.
        /// </summary>
        [Fact]
        public async Task Handle_ShouldDeleteAuctioneer_WhenAuctioneerExists()
        {
            // Arrange
            var auctioneerId = Guid.NewGuid();
            var auctioneer = new Auctioneers { UserId = UserId.Create(auctioneerId) };
            var users = new Users { UserId = auctioneerId };

            _auctioneerRepositoryMongoMock
                .Setup(repo => repo.GetAuctioneerByIdAsync(It.IsAny<UserId>()))
                .ReturnsAsync(auctioneer);

           _usersRepositoryMongoMock
                .Setup(repo => repo.GetUsersById(It.IsAny<Guid>()))
                .ReturnsAsync(users);

            _auctioneerRepositoryMock
                .Setup(repo => repo.DeleteAsync(It.IsAny<UserId>())) // ✅ Cambia `UserId` por `Guid`
                .ReturnsAsync(auctioneer);

            _keycloakServiceMock
                .Setup(service => service.DeleteUserAsync(It.IsAny<Guid>()))
                .ReturnsAsync("mocked-deletion-result"); // ✅ Devuelve un string simulado en lugar de `Task<string>`

            _usersRepositoryMock
                .Setup(repo => repo.DeleteUsersAsync(It.IsAny<UserId>()))
                .ReturnsAsync(users);

            // Act
            var result = await _handler.Handle(new DeleteAuctioneerCommand(UserId.Create(auctioneerId)), CancellationToken.None); // ✅ Usa `auctioneerId` sin `UserId.Create()`

            // Assert
            Assert.NotNull(result);
            Assert.Equal(auctioneerId, result.Value);
        }

        /// <summary>
        /// Verifica que cuando el subastador no existe, se lanza `UserNotFoundException`.
        /// </summary>
        [Fact]
        public async Task Handle_ShouldThrowUserNotFoundException_WhenAuctioneerDoesNotExist()
        {
            // Arrange
            var auctioneerId = Guid.NewGuid();

            _auctioneerRepositoryMongoMock
                .Setup(repo => repo.GetAuctioneerByIdAsync(It.IsAny<UserId>()))
                .ReturnsAsync((Auctioneers)null); // Simula que el subastador no existe

            // Act & Assert
            await Assert.ThrowsAsync<UserNotFoundException>(() => _handler.Handle(new DeleteAuctioneerCommand(auctioneerId), CancellationToken.None));
        }

        /// <summary>
        /// Verifica que cuando ocurre un error inesperado, la excepción se propaga correctamente.
        /// </summary>
        [Fact]
        public async Task Handle_ShouldThrowException_WhenUnexpectedErrorOccurs()
        {
            // Arrange
            var auctioneerId = Guid.NewGuid();

            _auctioneerRepositoryMongoMock
                .Setup(repo => repo.GetAuctioneerByIdAsync(It.IsAny<UserId>()))
                .ThrowsAsync(new Exception("Unexpected error"));

            // Act & Assert
            await Assert.ThrowsAsync<ApplicationException>(() => _handler.Handle(new DeleteAuctioneerCommand(auctioneerId), CancellationToken.None));
        }
    }

}
