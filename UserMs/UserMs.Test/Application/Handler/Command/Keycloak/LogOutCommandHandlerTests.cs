using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Application.Commands.Keycloak;
using UserMs.Application.Handlers.Keycloak;
using UserMs.Core.Service.Keycloak;
using Xunit;

namespace UserMs.Test.Application.Handler.Command.Keycloak
{
    public class LogOutCommandHandlerTests
    {
        private readonly Mock<IKeycloakService> _keycloakServiceMock;
        private readonly LogOutCommandHandler _handler;

        public LogOutCommandHandlerTests()
        {
            _keycloakServiceMock = new Mock<IKeycloakService>();
            _handler = new LogOutCommandHandler(_keycloakServiceMock.Object);
        }

      
        [Fact]
        public async Task Handle_ShouldReturnSuccessMessage_WhenLogOutIsSuccessful()
        {
            // Arrange
            var expectedMessage = "Usuario cerró sesión correctamente.";
            _keycloakServiceMock
                .Setup(service => service.LogOutAsync())
                .ReturnsAsync("logout_successful");

            // Act
            var result = await _handler.Handle(new LogOutCommand(), CancellationToken.None);

            // Assert
            Assert.Equal(expectedMessage, result);
        }

        [Fact]
        public async Task Handle_ShouldThrowException_WhenLogOutFails()
        {
            // Arrange
            _keycloakServiceMock
                .Setup(service => service.LogOutAsync())
                .ReturnsAsync(string.Empty);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _handler.Handle(new LogOutCommand(), CancellationToken.None));
        }

        
        [Fact]
        public async Task Handle_ShouldThrowException_WhenKeycloakServiceFails()
        {
            // Arrange
            _keycloakServiceMock
                .Setup(service => service.LogOutAsync())
                .ThrowsAsync(new HttpRequestException("Connection failed"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _handler.Handle(new LogOutCommand(), CancellationToken.None));
        }
    }

}
