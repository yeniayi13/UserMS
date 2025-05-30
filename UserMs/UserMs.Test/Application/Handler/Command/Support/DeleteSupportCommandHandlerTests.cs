using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserMs.Test.Application.Handler.Command.Support
{
    using Moq;
    using Xunit;
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using UserMs.Application.Commands.Support;
    using UserMs.Application.Handlers.Support.Command;
    using UserMs.Commoon.Dtos.Users.Response.ActivityHistory;
    using UserMs.Commoon.Dtos.Users.Response.Support;
    using UserMs.Commoon.Dtos.Users.Response.User;
    using UserMs.Core.RabbitMQ;
    using UserMs.Core.Repositories.ActivityHistoryRepo;
    using UserMs.Core.Repositories.Supports;
    using UserMs.Core.Repositories.SupportsRepo;
    using UserMs.Core.Repositories.UserRepo;
    using UserMs.Core.Service.Keycloak;
    using UserMs.Domain.Entities.Support;
    using UserMs.Domain.Entities.UserEntity;
    using UserMs.Domain.Entities;
    using UserMs.Infrastructure.Exceptions;

    public class DeleteSupportCommandHandlerTests
    {
        private readonly Mock<ISupportRepository> _supportRepositoryMock;
        private readonly Mock<ISupportRepositoryMongo> _supportRepositoryMongoMock;
        private readonly Mock<IEventBus<GetSupportDto>> _eventBusMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IKeycloakService> _keycloakServiceMock;
        private readonly Mock<IEventBus<GetUsersDto>> _eventBusUserMock;
        private readonly Mock<IUserRepository> _usersRepositoryMock;
        private readonly Mock<IUserRepositoryMongo> _usersRepositoryMongoMock;
        private readonly Mock<IActivityHistoryRepository> _activityHistoryRepositoryMock;
        private readonly Mock<IEventBus<GetActivityHistoryDto>> _eventBusActivityMock;
        private readonly DeleteSupportCommandHandler _handler;

        public DeleteSupportCommandHandlerTests()
        {
            _supportRepositoryMock = new Mock<ISupportRepository>();
            _supportRepositoryMongoMock = new Mock<ISupportRepositoryMongo>();
            _eventBusMock = new Mock<IEventBus<GetSupportDto>>();
            _mapperMock = new Mock<IMapper>();
            _keycloakServiceMock = new Mock<IKeycloakService>();
            _eventBusUserMock = new Mock<IEventBus<GetUsersDto>>();
            _usersRepositoryMock = new Mock<IUserRepository>();
            _usersRepositoryMongoMock = new Mock<IUserRepositoryMongo>();
            _activityHistoryRepositoryMock = new Mock<IActivityHistoryRepository>();
            _eventBusActivityMock = new Mock<IEventBus<GetActivityHistoryDto>>();

            _handler = new DeleteSupportCommandHandler(
                _eventBusActivityMock.Object,
                _activityHistoryRepositoryMock.Object,
                _supportRepositoryMock.Object,
                _supportRepositoryMongoMock.Object,
                _usersRepositoryMock.Object,
                _usersRepositoryMongoMock.Object,
                _keycloakServiceMock.Object,
                _eventBusUserMock.Object,
                _eventBusMock.Object,
                _mapperMock.Object
            );
        }

        /// <summary>
        /// Verifica que cuando el soporte existe, se elimina correctamente.
        /// </summary>
       /* [Fact]
        public async Task Handle_ShouldDeleteSupport_WhenSupportExists()
        {
            // Arrange
            var supportId = Guid.NewGuid();
            var support = new Supports { UserId = UserId.Create(supportId) };
            var users = new Users { UserId = supportId };

            _supportRepositoryMongoMock
                .Setup(repo => repo.GetSupportByIdAsync(It.IsAny<UserId>()))
                .ReturnsAsync(support);

            _usersRepositoryMongoMock
                .Setup(repo => repo.GetUsersById(It.IsAny<Guid>()))
                .ReturnsAsync(users);

            _supportRepositoryMock
                .Setup(repo => repo.DeleteAsync(It.IsAny<UserId>()))
                .ReturnsAsync(support);

            _keycloakServiceMock
                .Setup(service => service.DeleteUserAsync(It.IsAny<Guid>()))
                .ReturnsAsync("mocked-deletion-result");

            _usersRepositoryMock
                .Setup(repo => repo.DeleteUsersAsync(It.IsAny<UserId>()))
                .ReturnsAsync(users);

            // Act
            var result = await _handler.Handle(new DeleteSupportCommand(UserId.Create(supportId)), CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(supportId, result.Value);
        }*/

        /// <summary>
        /// Verifica que cuando el soporte no existe, se lanza `UserNotFoundException`.
        /// </summary>
        [Fact]
        public async Task Handle_ShouldThrowUserNotFoundException_WhenSupportDoesNotExist()
        {
            // Arrange
            var supportId = Guid.NewGuid();

            _supportRepositoryMongoMock
                .Setup(repo => repo.GetSupportByIdAsync(It.IsAny<UserId>()))
                .ReturnsAsync((Supports)null); // Simula que el soporte no existe

            // Act & Assert
            await Assert.ThrowsAsync<UserNotFoundException>(() => _handler.Handle(new DeleteSupportCommand(supportId), CancellationToken.None));
        }

        /// <summary>
        /// Verifica que cuando ocurre un error inesperado, la excepción se propaga correctamente.
        /// </summary>
        [Fact]
        public async Task Handle_ShouldThrowException_WhenUnexpectedErrorOccurs()
        {
            // Arrange
            var supportId = Guid.NewGuid();

            _supportRepositoryMongoMock
                .Setup(repo => repo.GetSupportByIdAsync(It.IsAny<UserId>()))
                .ThrowsAsync(new Exception("Unexpected error"));

            // Act & Assert
            await Assert.ThrowsAsync<ApplicationException>(() => _handler.Handle(new DeleteSupportCommand(supportId), CancellationToken.None));
        }
    }

}
