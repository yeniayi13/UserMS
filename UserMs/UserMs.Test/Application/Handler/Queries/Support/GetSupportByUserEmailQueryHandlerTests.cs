using AutoMapper;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Application.Handlers.Support.Queries;
using UserMs.Application.Queries.Support;
using UserMs.Commoon.Dtos.Users.Response.ActivityHistory;
using UserMs.Commoon.Dtos.Users.Response.Support;
using UserMs.Core.RabbitMQ;
using UserMs.Core.Repositories.ActivityHistoryRepo;
using UserMs.Core.Repositories.Supports;
using UserMs.Domain.Entities;
using UserMs.Domain.Entities.Support;
using UserMs.Infrastructure.Exceptions;
using Xunit;

namespace UserMs.Test.Application.Handler.Queries.Support
{
    public class GetSupportByUserEmailQueryHandlerTests
    {
        private readonly Mock<ISupportRepositoryMongo> _supportRepositoryMock;
        private readonly Mock<IActivityHistoryRepository> _activityHistoryRepositoryMock;
        private readonly Mock<IEventBus<GetActivityHistoryDto>> _eventBusActivityMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly GetSupportByUserEmailQueryHandler _handler;

        public GetSupportByUserEmailQueryHandlerTests()
        {
            _supportRepositoryMock = new Mock<ISupportRepositoryMongo>();
            _activityHistoryRepositoryMock = new Mock<IActivityHistoryRepository>();
            _eventBusActivityMock = new Mock<IEventBus<GetActivityHistoryDto>>();
            _mapperMock = new Mock<IMapper>();

            _handler = new GetSupportByUserEmailQueryHandler(
                _supportRepositoryMock.Object,
                _mapperMock.Object,
                _eventBusActivityMock.Object
            );
        }

        /// <summary>
        /// Verifica que cuando el trabajador de soporte existe, se devuelve correctamente el DTO mapeado.
        /// </summary>
        [Fact]
        public async Task Handle_ShouldReturnMappedSupport_WhenSupportExists()
        {
            // Arrange
            var email = UserEmail.Create("support1@example.com");
            var support = new Supports { UserId = Guid.NewGuid(), UserEmail = email };
            var expectedDto = new GetSupportDto { UserId = support.UserId, UserEmail = email.Value };
            var activityHistory = new Domain.Entities.ActivityHistory.ActivityHistory(Guid.NewGuid(), support.UserId, "Busco un trabajador de soporte por email", DateTime.UtcNow);
            var activityDto = new GetActivityHistoryDto();

            _supportRepositoryMock
                .Setup(repo => repo.GetSupportByEmailAsync(email))
                .ReturnsAsync(support);

            _mapperMock
                .Setup(mapper => mapper.Map<GetSupportDto>(support))
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
            var result = await _handler.Handle(new GetSupportByUserEmailQuery(email.Value), CancellationToken.None);

            // Assert
            Assert.Equal(expectedDto.UserId, result.UserId);
            Assert.Equal(expectedDto.UserEmail, result.UserEmail);
        }

        /// <summary>
        /// Verifica que cuando el trabajador de soporte no se encuentra, se lanza `UserNotFoundException`.
        /// </summary>
        [Fact]
        public async Task Handle_ShouldThrowUserNotFoundException_WhenSupportNotFound()
        {
            // Arrange
            var email = UserEmail.Create("notfound@example.com");

            _supportRepositoryMock
                .Setup(repo => repo.GetSupportByEmailAsync(email))
                .ReturnsAsync((Supports)null);

            // Act & Assert
            await Assert.ThrowsAsync<UserNotFoundException>(() => _handler.Handle(new GetSupportByUserEmailQuery(email.Value), CancellationToken.None));
        }

        /// <summary>
        /// Verifica que cuando ocurre una excepción en el repositorio, se propaga correctamente.
        /// </summary>
        [Fact]
        public async Task Handle_ShouldThrowException_WhenRepositoryFails()
        {
            // Arrange
            var email = UserEmail.Create("error@example.com");

            _supportRepositoryMock
                .Setup(repo => repo.GetSupportByEmailAsync(email))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _handler.Handle(new GetSupportByUserEmailQuery(email.Value), CancellationToken.None));
        }
    }

}
