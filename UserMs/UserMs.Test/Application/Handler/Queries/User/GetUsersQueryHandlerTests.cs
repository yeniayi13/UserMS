using AutoMapper;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Application.Handlers.User.Queries;
using UserMs.Application.Queries.User;
using UserMs.Commoon.Dtos.Users.Response.User;
using UserMs.Core.Repositories.UserRepo;
using UserMs.Domain.Entities;
using UserMs.Domain.Entities.IUser.ValueObjects;
using UserMs.Domain.Entities.UserEntity;
using UserMs.Infrastructure.Exceptions;
using Xunit;

namespace UserMs.Test.Application.Handler.Queries.User
{
    public class GetUsersQueryHandlerTests
    {
        private readonly Mock<IUserRepositoryMongo> _usersRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly GetUsersQueryHandler _handler;

        public GetUsersQueryHandlerTests()
        {
            _usersRepositoryMock = new Mock<IUserRepositoryMongo>();
            _mapperMock = new Mock<IMapper>();

            _handler = new GetUsersQueryHandler(
                _usersRepositoryMock.Object,
                _mapperMock.Object
            );
        }

        /// <summary>
        /// Verifica que cuando existen usuarios, se devuelven correctamente los datos mapeados.
        /// </summary>
        [Fact]
        public async Task Handle_ShouldReturnMappedUsers_WhenUsersExist()
        {
            // Arrange
            var userName = UserName.Create("Juan Pérez");
            var userName2 = UserName.Create("Ana Gómez");
            var users = new List<Users>
        {
            new Users { UserId = Guid.NewGuid(), UserName = userName, UserDelete = UserDelete.Create(false) },
            new Users { UserId = Guid.NewGuid(), UserName =userName2, UserDelete = UserDelete.Create(false) }
        };
            var expectedDtos = new List<GetUsersDto>
        {
            new GetUsersDto { UserId = users[0].UserId, UserName = users[0].UserName.Value },
            new GetUsersDto { UserId = users[1].UserId, UserName = users[1].UserName.Value }
        };

            _usersRepositoryMock
                .Setup(repo => repo.GetUsersAsync())
                .ReturnsAsync(users);

            _mapperMock
                .Setup(mapper => mapper.Map<List<GetUsersDto>>(users))
                .Returns(expectedDtos);

            // Act
            var result = await _handler.Handle(new GetUsersQuery(), CancellationToken.None);

            // Assert
            Assert.Equal(expectedDtos.Count, result.Count);
            Assert.Equal(expectedDtos[0].UserName, result[0].UserName);
            Assert.Equal(expectedDtos[1].UserName, result[1].UserName);
        }

        /// <summary>
        /// Verifica que cuando no hay usuarios, se retorne una lista vacía.
        /// </summary>
        [Fact]
        public async Task Handle_ShouldThrowUserNotFoundException_WhenNoUsersFound()
        {
            // Arrange
            _usersRepositoryMock
                .Setup(repo => repo.GetUsersAsync())
                .ReturnsAsync(new List<Users>()); // Simulación de que no hay usuarios

            // Act & Assert
            await Assert.ThrowsAsync<UserNotFoundException>(() => _handler.Handle(new GetUsersQuery(), CancellationToken.None));
        }

        /// <summary>
        /// Verifica que cuando ocurre una excepción en el repositorio, se propaga correctamente.
        /// </summary>
        [Fact]
        public async Task Handle_ShouldThrowException_WhenRepositoryFails()
        {
            // Arrange
            _usersRepositoryMock
                .Setup(repo => repo.GetUsersAsync())
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _handler.Handle(new GetUsersQuery(), CancellationToken.None));
        }
    }

}
