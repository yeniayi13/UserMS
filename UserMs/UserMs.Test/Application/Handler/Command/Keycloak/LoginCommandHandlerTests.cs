using AuthMs.Common.Exceptions;
using AutoMapper;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Application.Commands.Keycloak;
using UserMs.Application.Handlers.Keycloak;
using UserMs.Commoon.Dtos.Users.Response.ActivityHistory;
using UserMs.Commoon.Dtos;
using UserMs.Core.RabbitMQ;
using UserMs.Core.Repositories.ActivityHistoryRepo;
using UserMs.Core.Service.Keycloak;
using Xunit;
using UserMs.Core.Repositories.UserRepo;

namespace UserMs.Test.Application.Handler.Command.Keycloak
{
    public class LoginCommandHandlerTests
    {
        private readonly Mock<IKeycloakService> _keycloakServiceMock;
        private readonly Mock<IActivityHistoryRepository> _activityHistoryRepositoryMock;
        private readonly Mock<IEventBus<GetActivityHistoryDto>> _eventBusActivityMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly LoginCommandHandler _handler;
        private readonly Mock<IUserRepositoryMongo> _userMock;
        public LoginCommandHandlerTests()
        {
            _keycloakServiceMock = new Mock<IKeycloakService>();
            _activityHistoryRepositoryMock = new Mock<IActivityHistoryRepository>();
            _eventBusActivityMock = new Mock<IEventBus<GetActivityHistoryDto>>();
            _mapperMock = new Mock<IMapper>();
            _userMock = new Mock<IUserRepositoryMongo>();

            _handler = new LoginCommandHandler(
                _keycloakServiceMock.Object,
                _activityHistoryRepositoryMock.Object,
                _eventBusActivityMock.Object,
                _mapperMock.Object,
                _userMock.Object
            );
        }

        /// <summary>
        /// Verifica que cuando las credenciales son válidas, el token se devuelve correctamente.
        /// </summary>
        [Fact]
        public async Task Handle_ShouldReturnToken_WhenCredentialsAreValid()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var request = new LoginCommand(new LoginDto { Username = "testuser", Password = "password123" });
            var expectedToken = "valid_token";
            var activityHistory = new Domain.Entities.ActivityHistory.ActivityHistory(Guid.NewGuid(), userId, "Inicio Sesion", DateTime.UtcNow);
            var activityDto = new GetActivityHistoryDto();

            _keycloakServiceMock
                .Setup(service => service.GetUserByUserName(request.Login.Username))
                .ReturnsAsync(userId);

            _keycloakServiceMock
                .Setup(service => service.LoginAsync(request.Login.Username, request.Login.Password))
                .ReturnsAsync(expectedToken);

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
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.Equal(expectedToken, result);
        }

        /// <summary>
        /// Verifica que se lance una excepción cuando las credenciales son incorrectas.
        /// </summary>
        [Fact]
        public async Task Handle_ShouldThrowUnauthorizedAccessException_WhenCredentialsAreInvalid()
        {
            // Arrange
            var request = new LoginCommand(new LoginDto { Username = "testuser", Password = "wrongpassword" });

            _keycloakServiceMock
                .Setup(service => service.GetUserByUserName(request.Login.Username))
                .ReturnsAsync(Guid.NewGuid());

            _keycloakServiceMock
                .Setup(service => service.LoginAsync(request.Login.Username, request.Login.Password))
                .ReturnsAsync(string.Empty);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _handler.Handle(request, CancellationToken.None));
        }

        /// <summary>
        /// Verifica que se lance una excepción cuando el usuario no existe.
        /// </summary>
        [Fact]
        public async Task Handle_ShouldThrowKeycloakException_WhenUserDoesNotExist()
        {
            // Arrange
            var request = new LoginCommand(new LoginDto { Username = "nonexistentuser", Password = "password123" });

            _keycloakServiceMock
                .Setup(service => service.GetUserByUserName(request.Login.Username))
                .ReturnsAsync(Guid.Empty);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _handler.Handle(request, CancellationToken.None));
        }

        /// <summary>
        /// Verifica que se lance una excepción cuando hay un problema de conexión con el servidor de autenticación.
        /// </summary>
        [Fact]
        public async Task Handle_ShouldThrowException_WhenKeycloakServiceFails()
        {
            // Arrange
            var request = new LoginCommand(new LoginDto { Username = "testuser", Password = "password123" });

            _keycloakServiceMock
                .Setup(service => service.GetUserByUserName(request.Login.Username))
                .ThrowsAsync(new HttpRequestException("Connection failed"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _handler.Handle(request, CancellationToken.None));
        }
    }

}
