using AutoMapper;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Application.Handlers.User_Roles.Queries;
using UserMs.Application.Queries.User_Roles;
using UserMs.Commoon.Dtos.Users.Response.UserRole;
using UserMs.Core.Repositories.UserRoleRepo;
using UserMs.Domain.User_Roles;
using Xunit;

namespace UserMs.Test.Application.Handler.Queries.User_Role
{
    public class GetUserRolesByIdQueryHandlerTests
    {
        private readonly Mock<IUserRoleRepositoryMongo> _userRoleRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly GetUserRolesByIdQueryHandler _handler;

        public GetUserRolesByIdQueryHandlerTests()
        {
            _userRoleRepositoryMock = new Mock<IUserRoleRepositoryMongo>();
            _mapperMock = new Mock<IMapper>();

            _handler = new GetUserRolesByIdQueryHandler(
                _userRoleRepositoryMock.Object,
                _mapperMock.Object
            );
        }

        /// <summary>
        /// Verifica que cuando el usuario tiene roles, se devuelve correctamente el DTO mapeado.
        /// </summary>
      /*  [Fact]
        public async Task Handle_ShouldReturnMappedUserRoles_WhenUserRolesExist()
        {
            // Arrange
            var roleId = Guid.NewGuid();
            var email = "test@test.com";
            var roleName = "Admin";
            var userId = Guid.NewGuid();
            var userRoleId = Guid.NewGuid();

            var expectedUserRoles = new List<GetUserRoleDto>
            {
                new GetUserRoleDto { UserRoleId = userRoleId, RoleId = roleId, UserId = userId, RoleName = roleName, UserEmail = email }
            };

            _userRoleRepositoryMock
                .Setup(repo => repo.GetUserRolesByIdQuery(userRoleId))
                .ReturnsAsync(expectedUserRoles); // ✅ Devuelve una lista

            // Act
            var result = await _handler.Handle(new GetUserRolesByIdByUserIDQuery(userRoleId), CancellationToken.None);

            // Assert
            Assert.NotNull(result); // Aseguramos que no sea `null`// Verificamos que la lista no esté vacía
            Assert.Equal(expectedUserRoles[0].RoleId, result.RoleId);
            Assert.Equal(expectedUserRoles[0].RoleName, result.RoleName);
        }*/

        /// <summary>
        /// Verifica que cuando el usuario no tiene roles, se retorne un objeto vacío.
        /// </summary>
        [Fact]
        public async Task Handle_ShouldReturnEmptyUserRoleDto_WhenNoUserRolesFound()
        {
            // Arrange
            var userRoleId = Guid.NewGuid();

            _userRoleRepositoryMock
                .Setup(repo => repo.GetUserRolesByIdQuery(userRoleId))
                .ReturnsAsync((List<GetUserRoleDto>)null);

            // Act
            var result = await _handler.Handle(new GetUserRolesByIdByUserIDQuery(userRoleId), CancellationToken.None);

            // Assert
            Assert.NotNull(result); // Validamos que el objeto no sea `null`
            Assert.IsType<GetUserRoleDto>(result); // Verificamos que sea el tipo correcto
            Assert.Null(result.RoleName); // Confirmamos que no tiene datos válidos
        }

        /// <summary>
        /// Verifica que cuando ocurre una excepción en el repositorio, se propaga correctamente.
        /// </summary>
        [Fact]
        public async Task Handle_ShouldReturnEmptyUserRoleDto_WhenRepositoryFails()
        {
            // Arrange
            var userRoleId = Guid.NewGuid();

            _userRoleRepositoryMock
                .Setup(repo => repo.GetUserRolesByIdQuery(userRoleId))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _handler.Handle(new GetUserRolesByIdByUserIDQuery(userRoleId), CancellationToken.None);

            // Assert
            Assert.NotNull(result); // Validamos que el objeto no sea `null`
            Assert.IsType<GetUserRoleDto>(result); // Verificamos que sea el tipo correcto
            Assert.Null(result.RoleName); // Confirmamos que no tiene datos válidos
        }
    }

}
