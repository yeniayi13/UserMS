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
using UserMs.Commoon.Dtos.Keycloak;
using UserMs.Commoon.Dtos.Users.Response.ActivityHistory;
using UserMs.Core.RabbitMQ;
using UserMs.Core.Repositories.ActivityHistoryRepo;
using UserMs.Core.Service.Keycloak;
using Xunit;

namespace UserMs.Test.Application.Handler.Command.Keycloak
{
    public class ResetPasswordCommandHandlerTests
    {
        private readonly Mock<IKeycloakService> _keycloakServiceMock;
        private readonly Mock<IActivityHistoryRepository> _activityHistoryRepositoryMock;
        private readonly Mock<IEventBus<GetActivityHistoryDto>> _eventBusActivityMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly ResetPasswordCommandHandler _handler;

        public ResetPasswordCommandHandlerTests()
        {
            _keycloakServiceMock = new Mock<IKeycloakService>();
            _activityHistoryRepositoryMock = new Mock<IActivityHistoryRepository>();
            _eventBusActivityMock = new Mock<IEventBus<GetActivityHistoryDto>>();
            _mapperMock = new Mock<IMapper>();

            _handler = new ResetPasswordCommandHandler(
                _keycloakServiceMock.Object,
                _activityHistoryRepositoryMock.Object,
                _eventBusActivityMock.Object,
                _mapperMock.Object
            );
        }

    
        [Fact]
        public async Task Handle_ShouldReturnTrue_WhenPasswordResetSuccessful()
        {
            // Arrange
            var userEmail = "user@example.com";
            var request = new ResetPasswordCommand(new ResetPasswordDto { UserEmail = userEmail });
            var userId = Guid.NewGuid();
            var activityHistory = new Domain.Entities.ActivityHistory.ActivityHistory(Guid.NewGuid(), userId, "Solicitud de recuperación de contraseña", DateTime.UtcNow);
            var activityDto = new GetActivityHistoryDto();

            _keycloakServiceMock
                .Setup(service => service.GetUserByUserName(userEmail))
                .ReturnsAsync(userId);

            _keycloakServiceMock
                .Setup(service => service.SendPasswordResetEmailAsync(userEmail))
                .ReturnsAsync(true);

            _activityHistoryRepositoryMock
                .Setup(repo => repo.AddAsync(It.IsAny<Domain.Entities.ActivityHistory.ActivityHistory>()))
                .Returns(Task.CompletedTask);

            _mapperMock
                .Setup(mapper => mapper.Map<GetActivityHistoryDto>(It.IsAny<Domain.Entities.ActivityHistory.ActivityHistory>()))
                .Returns(activityDto);

            _eventBusActivityMock
                .Setup(bus => bus.PublishMessageAsync(activityDto, "activityHistoryQueue", "PASSWORD_RECOVERY_REQUESTED"))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task Handle_ShouldThrowArgumentException_WhenEmailIsEmpty()
        {
            // Arrange
            var request = new ResetPasswordCommand(new ResetPasswordDto { UserEmail = "" });

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _handler.Handle(request, CancellationToken.None));
        }

       
        [Fact]
        public async Task Handle_ShouldThrowKeycloakException_WhenUserDoesNotExist()
        {
            // Arrange
            var userEmail = "nonexistent@example.com";
            var request = new ResetPasswordCommand(new ResetPasswordDto { UserEmail = userEmail });

            _keycloakServiceMock
                .Setup(service => service.GetUserByUserName(userEmail))
                .ReturnsAsync(Guid.Empty);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _handler.Handle(request, CancellationToken.None));
        }

        
        [Fact]
        public async Task Handle_ShouldThrowException_WhenKeycloakFails()
        {
            // Arrange
            var userEmail = "user@example.com";
            var request = new ResetPasswordCommand(new ResetPasswordDto { UserEmail = userEmail });

            _keycloakServiceMock
                .Setup(service => service.SendPasswordResetEmailAsync(userEmail))
                .ReturnsAsync(false);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _handler.Handle(request, CancellationToken.None));
        }

        
        [Fact]
        public async Task Handle_ShouldThrowException_WhenHttpRequestFails()
        {
            // Arrange
            var userEmail = "user@example.com";
            var request = new ResetPasswordCommand(new ResetPasswordDto { UserEmail = userEmail });

            _keycloakServiceMock
                .Setup(service => service.GetUserByUserName(userEmail))
                .ThrowsAsync(new HttpRequestException("Connection failed"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _handler.Handle(request, CancellationToken.None));
        }
    }

}
