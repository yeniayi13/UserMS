using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Domain.Entities.Role_Permission;

namespace UserMs.Test.Application.Handler.Queries.Roles_Permission
{
    using Moq;
    using Xunit;
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using UserMs.Application.Handlers.Roles_Permission.Queries;
    using UserMs.Application.Queries.Roles_Permission;
    using UserMs.Commoon.Dtos.Users.Response.Role_Permission;
    using UserMs.Core.Repositories.RolePermissionRepo;

    public class GetRolesPermissionsAllQueryHandlerTests
    {
        private readonly Mock<IRolePermissionRepositoryMongo> _rolePermissionRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly GetRolesPermissionsAllQueryHandler _handler;

        public GetRolesPermissionsAllQueryHandlerTests()
        {
            _rolePermissionRepositoryMock = new Mock<IRolePermissionRepositoryMongo>();
            _mapperMock = new Mock<IMapper>();

            _handler = new GetRolesPermissionsAllQueryHandler(
                _rolePermissionRepositoryMock.Object,
                _mapperMock.Object
            );
        }

        /// <summary>
        /// Verifica que cuando hay permisos de rol, se devuelvan correctamente los datos mapeados.
        /// </summary>
      //  [Fact]
    /*    public async Task Handle_ShouldReturnMappedRolePermissions_WhenRolePermissionsExist()
        {
            // Arrange
            var rolePermissions = new List<RolePermissions>
        {
            new RolePermissions { RolePermissionId = Guid.NewGuid() },
            new RolePermissions { RolePermissionId = Guid.NewGuid() }
        };
            var expectedDtos = new List<GetRolePermissionDto>
        {
            new GetRolePermissionDto { RolePermissionId = rolePermissions[0].RolePermissionId },
            new GetRolePermissionDto { RolePermissionId = rolePermissions[1].RolePermissionId}
        };

            _rolePermissionRepositoryMock
                .Setup(repo => repo.GetRolesPermissionAsync())
                .ReturnsAsync(expectedDtos);

            _mapperMock
                .Setup(mapper => mapper.Map<List<GetRolePermissionDto>>(rolePermissions))
                .Returns(expectedDtos);

            // Act
            var result = await _handler.Handle(new GetRolesPermissionsAllQuery(), CancellationToken.None);

            // Assert
            Assert.Equal(expectedDtos.Count, result.Count);
            Assert.Equal(expectedDtos[0].RolePermissionId, result[0].RolePermissionId);
            Assert.Equal(expectedDtos[1].RolePermissionId, result[1].RolePermissionId);
        }*/

        /// <summary>
        /// Verifica que cuando no hay permisos de rol, se retorne una lista vacía.
        /// </summary>
        [Fact]
        public async Task Handle_ShouldReturnEmptyList_WhenNoRolePermissionsFound()
        {
            // Arrange
            _rolePermissionRepositoryMock
                .Setup(repo => repo.GetRolesPermissionAsync())
                .ReturnsAsync(new List<GetRolePermissionDto>()); // Lista vacía

            _mapperMock
                .Setup(mapper => mapper.Map<List<GetRolePermissionDto>>(It.IsAny<List<GetRolePermissionDto>>()))
                .Returns(new List<GetRolePermissionDto>()); // Simulación de mapeo

            // Act
            var result = await _handler.Handle(new GetRolesPermissionsAllQuery(), CancellationToken.None);

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
            _rolePermissionRepositoryMock
                .Setup(repo => repo.GetRolesPermissionAsync())
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _handler.Handle(new GetRolesPermissionsAllQuery(), CancellationToken.None));
        }
    }
}
