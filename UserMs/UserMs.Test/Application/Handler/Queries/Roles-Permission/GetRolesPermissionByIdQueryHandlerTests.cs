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
using UserMs.Infrastructure.Exceptions;
using Xunit;

namespace UserMs.Test.Application.Handler.Queries.Roles_Permission
{
    public class GetRolesPermissionByIdQueryHandlerTests
    {
        private readonly Mock<IRolePermissionRepositoryMongo> _rolePermissionRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly GetRolesPermissionByIdQueryHandler _handler;

        public GetRolesPermissionByIdQueryHandlerTests()
        {
            _rolePermissionRepositoryMock = new Mock<IRolePermissionRepositoryMongo>();
            _mapperMock = new Mock<IMapper>();

            _handler = new GetRolesPermissionByIdQueryHandler(
                _rolePermissionRepositoryMock.Object,
                _mapperMock.Object
            );
        }

        /// <summary>
        /// Verifica que cuando el permiso de rol existe, se devuelve correctamente el DTO mapeado.
        /// </summary>
      /*  [Fact]
        public async Task Handle_ShouldReturnMappedRolePermission_WhenRolePermissionExists()
        {
            // Arrange
            var rolePermissionId = Guid.NewGuid();
            var rolePermission = new RolePermissions { RolePermissionId = rolePermissionId };
            var expectedDto = new GetRolePermissionDto { RolePermissionId = rolePermissionId };

            _rolePermissionRepositoryMock
                .Setup(repo => repo.GetRolesPermissionByIdQuery(rolePermissionId))
                .ReturnsAsync(expectedDto);

            _mapperMock
                .Setup(mapper => mapper.Map<GetRolePermissionDto>(rolePermission))
                .Returns(expectedDto);

            // Act
            var result = await _handler.Handle(new GetRolesPermissionByIdQuery(rolePermissionId), CancellationToken.None);

            // Assert
            Assert.Equal(expectedDto.RolePermissionId, result.RolePermissionId);
          
        }*/

        /// <summary>
        /// Verifica que cuando el permiso de rol no se encuentra, se lanza `RolePermissionNotFoundException`.
        /// </summary>
        [Fact]
        public async Task Handle_ShouldThrowRolePermissionNotFoundException_WhenRolePermissionNotFound()
        {
            // Arrange
            var rolePermissionId = Guid.NewGuid();
            _rolePermissionRepositoryMock
                .Setup(repo => repo.GetRolesPermissionByIdQuery(rolePermissionId))
                .ReturnsAsync((GetRolePermissionDto)null); // Simulamos que el repositorio devuelve `null`

            // Act & Assert
            await Assert.ThrowsAsync<RolePermissionNotFoundException>(() => _handler.Handle(new GetRolesPermissionByIdQuery(rolePermissionId), CancellationToken.None));
        }

        /// <summary>
        /// Verifica que cuando ocurre una excepción en el repositorio, se propaga correctamente.
        /// </summary>
        [Fact]
        public async Task Handle_ShouldThrowException_WhenRepositoryFails()
        {
            // Arrange
            var rolePermissionId = Guid.NewGuid();

            _rolePermissionRepositoryMock
                .Setup(repo => repo.GetRolesPermissionByIdQuery(rolePermissionId))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _handler.Handle(new GetRolesPermissionByIdQuery(rolePermissionId), CancellationToken.None));
        }
    }

}
