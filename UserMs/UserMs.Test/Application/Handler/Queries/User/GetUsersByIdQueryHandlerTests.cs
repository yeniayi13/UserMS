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
    public class GetUsersByIdQueryHandlerTests
    {
        private readonly Mock<IUserRepositoryMongo> _usersRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly GetUsersByIdQueryHandler _handler;

        public GetUsersByIdQueryHandlerTests()
        {
            _usersRepositoryMock = new Mock<IUserRepositoryMongo>();
            _mapperMock = new Mock<IMapper>();

            _handler = new GetUsersByIdQueryHandler(
                _usersRepositoryMock.Object,
                _mapperMock.Object
            );
        }

        /// <summary>
        /// Verifica que cuando el usuario existe, se devuelve correctamente el DTO mapeado.
        /// </summary>
      /*  [Fact]
        public async Task Handle_ShouldReturnMappedUser_WhenUserExists()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var userName = UserName.Create("Juan Pérez");
            var user = new Users { UserId = userId, UserName = userName };
            var expectedDto = new GetUsersDto { UserId = userId, UserName = userName.Value };

            _usersRepositoryMock
                .Setup(repo => repo.GetUsersById(userId))
                .ReturnsAsync(user);

            _mapperMock
                .Setup(mapper => mapper.Map<GetUsersDto>(user))
                .Returns(expectedDto);

            // Act
            var result = await _handler.Handle(new GetUsersByIdQuery(userId), CancellationToken.None);

            // Assert
            Assert.Equal(expectedDto.UserId, result.UserId);
            Assert.Equal(expectedDto.UserName, result.UserName);
        }*/

        /// <summary>
        /// Verifica que cuando el usuario no se encuentra o está eliminado, se lanza `UserNotFoundException`.
        /// </summary>
        [Fact]
        public async Task Handle_ShouldThrowUserNotFoundException_WhenUserNotFoundOrDeleted()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var userName = UserName.Create("Usuario Eliminado");
            var deletedUser = new Users { UserId = userId, UserName = userName,UserDelete = UserDelete.Create(true) };

            _usersRepositoryMock
                .Setup(repo => repo.GetUsersById(userId))
                .ReturnsAsync((Users)null);

            // Act & Assert
            await Assert.ThrowsAsync<UserNotFoundException>(() => _handler.Handle(new GetUsersByIdQuery(userId), CancellationToken.None));

            // Arrange para usuario eliminado
            _usersRepositoryMock
                .Setup(repo => repo.GetUsersById(userId))
                .ReturnsAsync(deletedUser);

            // Act & Assert
            await Assert.ThrowsAsync<UserNotFoundException>(() => _handler.Handle(new GetUsersByIdQuery(userId), CancellationToken.None));
        }

        /// <summary>
        /// Verifica que cuando ocurre una excepción en el repositorio, se propaga correctamente.
        /// </summary>
        [Fact]
        public async Task Handle_ShouldThrowException_WhenRepositoryFails()
        {
            // Arrange
            var userId = Guid.NewGuid();

            _usersRepositoryMock
                .Setup(repo => repo.GetUsersById(userId))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _handler.Handle(new GetUsersByIdQuery(userId), CancellationToken.None));
        }
    }

}
