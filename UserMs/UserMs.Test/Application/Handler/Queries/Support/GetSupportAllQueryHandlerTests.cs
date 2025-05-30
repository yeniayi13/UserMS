using AutoMapper;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Application.Handlers.Support.Queries;
using UserMs.Application.Queries.Support;
using UserMs.Commoon.Dtos.Users.Response.Support;
using UserMs.Core.Repositories.Supports;
using UserMs.Domain.Entities.IUser.ValueObjects;
using UserMs.Domain.Entities.Support;
using UserMs.Infrastructure.Exceptions;
using Xunit;

namespace UserMs.Test.Application.Handler.Queries.Support
{
    public class GetSupportAllQueryHandlerTests
    {
        private readonly Mock<ISupportRepositoryMongo> _supportRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly GetSupportAllQueryHandler _handler;

        public GetSupportAllQueryHandlerTests()
        {
            _supportRepositoryMock = new Mock<ISupportRepositoryMongo>();
            _mapperMock = new Mock<IMapper>();

            _handler = new GetSupportAllQueryHandler(
                _supportRepositoryMock.Object,
                _mapperMock.Object
            );
        }

        /// <summary>
        /// Verifica que cuando hay registros de soporte, se devuelvan correctamente los datos mapeados.
        /// </summary>
        [Fact]
        public async Task Handle_ShouldReturnMappedSupports_WhenSupportsExist()
        {
            // Arrange
            var name1 = UserName.Create("Soporte 1");
            var name2 = UserName.Create("Soporte 2");
            var supports = new List<Supports>
        {
            new Supports { UserId = Guid.NewGuid(), UserName = name1 },
            new Supports { UserId = Guid.NewGuid(), UserName = name2 }
        };
            var expectedDtos = new List<GetSupportDto>
        {
            new GetSupportDto { UserId = supports[0].UserId, UserName = supports[0].UserName.Value },
            new GetSupportDto { UserId = supports[1].UserId, UserName = supports[1].UserName.Value }
        };

            _supportRepositoryMock
                .Setup(repo => repo.GetSupportAllAsync())
                .ReturnsAsync(supports);

            _mapperMock
                .Setup(mapper => mapper.Map<List<GetSupportDto>>(supports))
                .Returns(expectedDtos);

            // Act
            var result = await _handler.Handle(new GetSupportAllQuery(), CancellationToken.None);

            // Assert
            Assert.Equal(expectedDtos.Count, result.Count);
            Assert.Equal(expectedDtos[0].UserName, result[0].UserName);
            Assert.Equal(expectedDtos[1].UserName, result[1].UserName);
        }

        /// <summary>
        /// Verifica que cuando no hay registros de soporte, se retorne una lista vacía.
        /// </summary>
        [Fact]
        public async Task Handle_ShouldThrowUserNotFoundException_WhenNoSupportsFound()
        {
            // Arrange
            _supportRepositoryMock
                .Setup(repo => repo.GetSupportAllAsync())
                .ReturnsAsync((List<Supports>)null); // Simulamos que la respuesta es null

            // Act & Assert
            await Assert.ThrowsAsync<UserNotFoundException>(() => _handler.Handle(new GetSupportAllQuery(), CancellationToken.None));
        }

        /// <summary>
        /// Verifica que cuando ocurre una excepción en el repositorio, se propaga correctamente.
        /// </summary>
        [Fact]
        public async Task Handle_ShouldThrowException_WhenRepositoryFails()
        {
            // Arrange
            _supportRepositoryMock
                .Setup(repo => repo.GetSupportAllAsync())
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _handler.Handle(new GetSupportAllQuery(), CancellationToken.None));
        }
    }

}
