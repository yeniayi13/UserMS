using AutoMapper;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Application.Handlers.User_Roles.Queries___Copia;
using UserMs.Application.Queries.Roles;
using UserMs.Commoon.Dtos.Users.Response.UserRole;
using UserMs.Core.Repositories.UserRoleRepo;
using UserMs.Domain.User_Roles;
using UserMs.Infrastructure.Exceptions;
using Xunit;

namespace UserMs.Test.Application.Handler.Queries.User_Role
{
    public class GetRoleByRoleNameAndByUserEmailHandlerTests
    {
        private readonly Mock<IUserRoleRepositoryMongo> _userRoleRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly GetRoleByRoleNameAndByUserEmailQueryHandler _handler;

        public GetRoleByRoleNameAndByUserEmailHandlerTests()
        {
            _userRoleRepositoryMock = new Mock<IUserRoleRepositoryMongo>();
            _mapperMock = new Mock<IMapper>();

            _handler = new GetRoleByRoleNameAndByUserEmailQueryHandler(
                _userRoleRepositoryMock.Object,
                _mapperMock.Object
            );
        }

        /// <summary>
        /// Verifica que cuando el rol existe, se devuelve `true`.
        /// </summary>
        [Fact]
        public async Task Handle_ShouldReturnTrue_WhenRoleExists()
        {
            // Arrange
            var roleId = Guid.NewGuid();
            var email = "test@test.com";
            var roleName = "Admin";
            var userId = Guid.NewGuid();
            var userRole = new GetUserRoleDto { RoleId = roleId, UserId = userId,RoleName = roleName,UserEmail = roleName };

            _userRoleRepositoryMock
                .Setup(repo => repo.GetRoleByRoleNameAndByUserEmail(roleName, email))
                .ReturnsAsync(userRole);

            // Act
            var result = await _handler.Handle(new GetRoleByIdAndByUserIdQuery(roleName, email), CancellationToken.None);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Verifica que cuando el rol no se encuentra, se lanza `RoleNotFoundException`.
        /// </summary>
        [Fact]
        public async Task Handle_ShouldThrowRoleNotFoundException_WhenRoleNotFound()
        {
            // Arrange
            var roleId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var email = "test@test.com";
            var roleName = "Admin";
            _userRoleRepositoryMock
                .Setup(repo => repo.GetRoleByRoleNameAndByUserEmail(roleName, email))
                .ReturnsAsync((GetUserRoleDto)null);

            // Act & Assert
            await Assert.ThrowsAsync<RoleNotFoundException>(() => _handler.Handle(new GetRoleByIdAndByUserIdQuery(roleName, email), CancellationToken.None));
        }

        /// <summary>
        /// Verifica que cuando ocurre una excepción en el repositorio, se propaga correctamente.
        /// </summary>
        [Fact]
        public async Task Handle_ShouldThrowException_WhenRepositoryFails()
        {
            // Arrange
            var roleId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var email = "test@test.com";
            var roleName = "Admin";
            _userRoleRepositoryMock
                .Setup(repo => repo.GetRoleByRoleNameAndByUserEmail(roleName, email))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _handler.Handle(new GetRoleByIdAndByUserIdQuery(roleName, email), CancellationToken.None));
        }
    }

}
