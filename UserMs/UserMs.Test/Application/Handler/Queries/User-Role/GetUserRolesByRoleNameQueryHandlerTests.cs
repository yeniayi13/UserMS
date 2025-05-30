using AutoMapper;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Application.Handlers.User_Roles.Queries___Copia;
using UserMs.Application.Queries.User_Roles;
using UserMs.Commoon.Dtos.Users.Response.UserRole;
using UserMs.Core.Repositories.UserRoleRepo;
using UserMs.Domain.User_Roles;
using Xunit;

namespace UserMs.Test.Application.Handler.Queries.User_Role
{
    public class GetUserRolesByRoleNameQueryHandlerTests
    {
        private readonly Mock<IUserRoleRepositoryMongo> _userRoleRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly GetUserRolesByRoleNameQueryHandler _handler;

        public GetUserRolesByRoleNameQueryHandlerTests()
        {
            _userRoleRepositoryMock = new Mock<IUserRoleRepositoryMongo>();
            _mapperMock = new Mock<IMapper>();

            _handler = new GetUserRolesByRoleNameQueryHandler(
                _userRoleRepositoryMock.Object,
                _mapperMock.Object
            );
        }

        /// <summary>
        /// Verifica que cuando existen usuarios con un rol, se devuelven correctamente los datos mapeados.
        /// </summary>
      /*  [Fact]
        public async Task Handle_ShouldReturnMappedUserRoles_WhenUserRolesExist()
        {
            // Arrange
            var roleName = "Administrator";
            var expectedDtos = new List<GetUserRoleDto>
            {
                new() { UserRoleId = Guid.NewGuid(), UserId = Guid.NewGuid(), RoleName = roleName },
                new() { UserRoleId = Guid.NewGuid(), UserId = Guid.NewGuid(), RoleName = roleName }
            };

            _userRoleRepositoryMock
                .Setup(repo => repo.GetUserRolesByRoleNameQuery(roleName))
                .ReturnsAsync(expectedDtos); // ✅ Aquí se usa directamente `expectedDtos`, ya que es el tipo que devuelve el repositorio.

            // Act
            var result = await _handler.Handle(new GetUserRolesByRoleNameQuery(roleName), CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedDtos.Count, result.Count);
            Assert.Equal(expectedDtos[0].UserRoleId, result[0].UserRoleId);
            Assert.Equal(expectedDtos[1].UserRoleId, result[1].UserRoleId);
        }*/

        /// <summary>
        /// Verifica que cuando no hay usuarios con el rol especificado, se retorne una lista vacía.
        /// </summary>
        [Fact]
        public async Task Handle_ShouldReturnEmptyList_WhenNoUserRolesFound()
        {
            // Arrange
            var roleName = "NonExistentRole";

            _userRoleRepositoryMock
                .Setup(repo => repo.GetUserRolesByRoleNameQuery(roleName))
                .ReturnsAsync(new List<GetUserRoleDto>()); // Lista vacía

            _mapperMock
                .Setup(mapper => mapper.Map<List<GetUserRoleDto>>(It.IsAny<List<GetUserRoleDto>>()))
                .Returns(new List<GetUserRoleDto>()); // Simulación de mapeo

            // Act
            var result = await _handler.Handle(new GetUserRolesByRoleNameQuery(roleName), CancellationToken.None);

            // Assert
            Assert.NotNull(result); // Validamos que la respuesta no sea `null`
            Assert.Empty(result); // Verificamos que la lista esté vacía
        }

        /// <summary>
        /// Verifica que cuando ocurre una excepción en el repositorio, se propaga correctamente.
        /// </summary>
        [Fact]
        public async Task Handle_ShouldThrowException_WhenRepositoryFails()
        {
            // Arrange
            var roleName = "AnyRole";

            _userRoleRepositoryMock
                .Setup(repo => repo.GetUserRolesByRoleNameQuery(roleName))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _handler.Handle(new GetUserRolesByRoleNameQuery(roleName), CancellationToken.None));
        }
    }

}
