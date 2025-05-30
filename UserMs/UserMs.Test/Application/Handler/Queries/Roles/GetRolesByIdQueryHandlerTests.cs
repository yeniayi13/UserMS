using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserMs.Test.Application.Handler.Queries.Roles
{
    using Moq;
    using Xunit;
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using UserMs.Application.Handlers.Roles.Queries;
    using UserMs.Application.Queries.Roles;
    using UserMs.Commoon.Dtos.Users.Response.Role;
    using UserMs.Core.Repositories.RolesRepo;
    using UserMs.Infrastructure.Exceptions;
    using UserMs.Domain.Entities.Role.ValueObjects;

    public class GetRolesByIdQueryHandlerTests
    {
        private readonly Mock<IRolesRepository> _rolesRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly GetRolesByIdQueryHandler _handler;

        public GetRolesByIdQueryHandlerTests()
        {
            _rolesRepositoryMock = new Mock<IRolesRepository>();
            _mapperMock = new Mock<IMapper>();

            _handler = new GetRolesByIdQueryHandler(
                _rolesRepositoryMock.Object,
                _mapperMock.Object
            );
        }

  
        [Fact]
        public async Task Handle_ShouldReturnMappedRole_WhenRoleExists()
        {
            // Arrange
            var roleId = Guid.NewGuid();
            var admin = RoleName.Create("Admin");
            var role = new Domain.Entities.Role.Roles { RoleId = roleId, RoleName = admin };
            var expectedDto = new GetRoleDto { RoleId = roleId, RoleName = admin.Value };

            _rolesRepositoryMock
                .Setup(repo => repo.GetRolesByIdQuery(roleId))
                .ReturnsAsync(role);

            _mapperMock
                .Setup(mapper => mapper.Map<GetRoleDto>(role))
                .Returns(expectedDto);

            // Act
            var result = await _handler.Handle(new GetRolesByIdQuery(roleId), CancellationToken.None);

            // Assert
            Assert.Equal(expectedDto.RoleId, result.RoleId);
            Assert.Equal(expectedDto.RoleName, result.RoleName);
        }

     
        [Fact]
        public async Task Handle_ShouldThrowRoleNotFoundException_WhenRoleDoesNotExist()
        {
            // Arrange
            var roleId = Guid.NewGuid();

            _rolesRepositoryMock
                .Setup(repo => repo.GetRolesByIdQuery(roleId))
                .ReturnsAsync((Domain.Entities.Role.Roles)null); // Simulamos que no se encuentra el rol.

            // Act & Assert
            await Assert.ThrowsAsync<RoleNotFoundException>(() => _handler.Handle(new GetRolesByIdQuery(roleId), CancellationToken.None));
        }

   
        [Fact]
        public async Task Handle_ShouldThrowException_WhenRepositoryFails()
        {
            // Arrange
            var roleId = Guid.NewGuid();

            _rolesRepositoryMock
                .Setup(repo => repo.GetRolesByIdQuery(roleId))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _handler.Handle(new GetRolesByIdQuery(roleId), CancellationToken.None));
        }
    }
}
