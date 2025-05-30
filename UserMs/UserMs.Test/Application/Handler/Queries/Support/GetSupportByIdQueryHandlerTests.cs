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
using UserMs.Domain.Entities.IUser.ValueObjects;
using UserMs.Domain.Entities.Support;
using UserMs.Infrastructure.Exceptions;
using Xunit;

namespace UserMs.Test.Application.Handler.Queries.Support
{
    public class GetSupportByIdQueryHandlerTests
    {
        private readonly Mock<ISupportRepositoryMongo> _supportRepositoryMock;
        private readonly Mock<IActivityHistoryRepository> _activityHistoryRepositoryMock;
        private readonly Mock<IEventBus<GetActivityHistoryDto>> _eventBusActivityMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly GetSupportByIdQueryHandler _handler;

        public GetSupportByIdQueryHandlerTests()
        {
            _supportRepositoryMock = new Mock<ISupportRepositoryMongo>();
            _activityHistoryRepositoryMock = new Mock<IActivityHistoryRepository>();
            _eventBusActivityMock = new Mock<IEventBus<GetActivityHistoryDto>>();
            _mapperMock = new Mock<IMapper>();

            _handler = new GetSupportByIdQueryHandler(
                _supportRepositoryMock.Object,
                _mapperMock.Object,
                _eventBusActivityMock.Object
            );
        }

        /// <summary>
        /// Verifica que cuando el trabajador de soporte existe, se devuelve correctamente el DTO mapeado.
        /// </summary>
     /*   [Fact]
        public async Task Handle_ShouldReturnMappedSupport_WhenSupportExists()
        {
            // Arrange
            var name1 = UserName.Create("Soporte 1");
            var supportId = Guid.NewGuid();
            var support = new Supports { UserId = supportId, UserName = name1 };
            var expectedDto = new GetSupportDto { UserId = supportId, UserName = name1.Value };
            var activityHistory = new Domain.Entities.ActivityHistory.ActivityHistory(Guid.NewGuid(), supportId, "Busco un trabajador de soporte por id", DateTime.UtcNow);
            var activityDto = new GetActivityHistoryDto();

            _supportRepositoryMock
                .Setup(repo => repo.GetSupportByIdAsync(It.IsAny<UserId>()))
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

            // Validaciones previas
            var repoResult = await _supportRepositoryMock.Object.GetSupportByIdAsync(supportId);
            Assert.NotNull(repoResult); // 🛠 Validamos que el repositorio no devuelve `null`

            var mappedResult = _mapperMock.Object.Map<GetSupportDto>(support);
            Assert.NotNull(mappedResult); // 🛠 Validamos que el mapeo no está generando `null`

            // Act
            var result = await _handler.Handle(new GetSupportByIdQuery(supportId), CancellationToken.None);

            // Assert
            Assert.NotNull(result); // 🛠 Aseguramos que el handler devuelve datos válidos
            Assert.Equal(expectedDto.UserId, result.UserId);
            Assert.Equal(expectedDto.UserName, result.UserName);
        }*/

        /// <summary>
        /// Verifica que cuando el trabajador de soporte no se encuentra, se retorne `null`.
        /// </summary>
        [Fact]
        public async Task Handle_ShouldThrowUserNotFoundException_WhenSupportNotFound()
        {
            // Arrange
            var supportId = Guid.NewGuid();

            _supportRepositoryMock
                .Setup(repo => repo.GetSupportByIdAsync(It.IsAny<UserId>()))
                .ReturnsAsync((Supports)null); // Simulamos que el repositorio devuelve `null`

            // Act & Assert
            await Assert.ThrowsAsync<UserNotFoundException>(() => _handler.Handle(new GetSupportByIdQuery(supportId), CancellationToken.None));
        }

        /// <summary>
        /// Verifica que cuando ocurre una excepción, se maneja correctamente y se retorna `null`.
        /// </summary>
        [Fact]
        public async Task Handle_ShouldThrowException_WhenRepositoryFails()
        {
            // Arrange
            var supportId = Guid.NewGuid();

            _supportRepositoryMock
                .Setup(repo => repo.GetSupportByIdAsync(It.IsAny<UserId>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _handler.Handle(new GetSupportByIdQuery(supportId), CancellationToken.None));
        }
    }

}
