using AutoMapper;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Application.Handlers.Roles_Permission.Queries;
using UserMs.Application.Queries.Roles_Permission;
using UserMs.Commoon.Dtos.Users.Response.Role_Permission;
using UserMs.Core.Repositories.RolePermissionRepo;
using UserMs.Domain.Entities.Role_Permission;
using UserMs.Domain.Entities.Role_Permission.ValueObjects;
using Xunit;

namespace UserMs.Test.Application.Handler.Queries.Roles_Permission
{
    public class GetRolesPermissionByRoleQueryHandlerTests
    {
        private readonly Mock<IRolePermissionRepositoryMongo> _rolePermissionRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly GetRolesPermissionByRoleQueryHandler _handler;

        public GetRolesPermissionByRoleQueryHandlerTests()
        {
            _rolePermissionRepositoryMock = new Mock<IRolePermissionRepositoryMongo>();
            _mapperMock = new Mock<IMapper>();

            _handler = new GetRolesPermissionByRoleQueryHandler(
                _rolePermissionRepositoryMock.Object,
                _mapperMock.Object
            );
        }

        /// <summary>
        /// Verifica que cuando hay permisos de rol, se devuelvan correctamente los datos mapeados.
        /// </summary>
      /*  [Fact]
        public async Task Handle_ShouldReturnMappedRolePermission_WhenRolePermissionExists()
        {
            // Arrange
            var roleName = "Administrator";

            var expectedDto = new GetRolePermissionDto
            {
                RolePermissionId = Guid.NewGuid()
            };

            _rolePermissionRepositoryMock
                .Setup(repo => repo.GetRolesPermissionByRoleQuery(roleName))
                .ReturnsAsync(expectedDto); // ✅ Devuelve un solo objeto en lugar de una lista

            // Act
            var result = await _handler.Handle(new GetRolesPermissionByRoleQuery(roleName), CancellationToken.None);

            // Assert
            Assert.NotNull(result); // ✅ Aseguramos que no sea `null`
            Assert.Equal(expectedDto.RolePermissionId, result[0].RolePermissionId);
        }*/

        /// <summary>
        /// Verifica que cuando no hay permisos para el rol, se retorne una lista vacía.
        /// </summary>
       /* [Fact]
        public async Task Handle_ShouldReturnEmptyList_WhenNoRolePermissionsFound()
        {
            // Arrange
            var roleName = "NonExistentRole";
            List<GetRolePermissionDto> rolePermissions = new List<GetRolePermissionDto>(); // Inicialización correcta
            _rolePermissionRepositoryMock
                .Setup(repo => repo.GetRolesPermissionByRoleQuery(roleName))
                .ReturnsAsync(List<GetRolePermissionDto>); // Lista vacía

            // Act
            var result = await _handler.Handle(new GetRolesPermissionByRoleQuery(roleName), CancellationToken.None);

            // Assert
            Assert.NotNull(result); // Validamos que la respuesta no sea `null`
            Assert.Empty(result); // Verificamos que la lista esté vacía
        }*/

        /// <summary>
        /// Verifica que cuando ocurre una excepción en el repositorio, se propaga correctamente.
        /// </summary>
        [Fact]
        public async Task Handle_ShouldThrowException_WhenRepositoryFails()
        {
            // Arrange
            var roleName = "AnyRole";

            _rolePermissionRepositoryMock
                .Setup(repo => repo.GetRolesPermissionByRoleQuery(roleName))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _handler.Handle(new GetRolesPermissionByRoleQuery(roleName), CancellationToken.None));
        }
    }

}
