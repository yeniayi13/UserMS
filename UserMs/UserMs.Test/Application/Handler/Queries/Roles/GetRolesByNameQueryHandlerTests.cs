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

    public class GetRolesByNameQueryHandlerTests
    {
        private readonly Mock<IRolesRepository> _rolesRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly GetRolesByNameQueryHandler _handler;

        public GetRolesByNameQueryHandlerTests()
        {
            _rolesRepositoryMock = new Mock<IRolesRepository>();
            _mapperMock = new Mock<IMapper>();

            _handler = new GetRolesByNameQueryHandler(
                _rolesRepositoryMock.Object,
                _mapperMock.Object
            );
        }

        
        [Fact]
        public async Task Handle_ShouldReturnMappedRole_WhenRoleExists()
        {
            // Arrange
            var admin = RoleName.Create("Admin");
            var role = new Domain.Entities.Role.Roles { RoleId = Guid.NewGuid(), RoleName = admin };
            var expectedDto = new GetRoleDto { RoleId = role.RoleId, RoleName = admin.Value };

            _rolesRepositoryMock
                .Setup(repo => repo.GetRolesByNameQuery(admin.Value))
                .ReturnsAsync(role);

            _mapperMock
                .Setup(mapper => mapper.Map<GetRoleDto>(role))
                .Returns(expectedDto);

            // Act
            var result = await _handler.Handle(new GetRolesByNameQuery(admin.Value), CancellationToken.None);

            // Assert
            Assert.Equal(expectedDto.RoleId, result.RoleId);
            Assert.Equal(expectedDto.RoleName, result.RoleName);
        }

        
        [Fact]
        public async Task Handle_ShouldThrowRoleNotFoundException_WhenRoleDoesNotExist()
        {
            // Arrange
            var roleName = "NonExistentRole";

            _rolesRepositoryMock
                .Setup(repo => repo.GetRolesByNameQuery(roleName))
                .ReturnsAsync((Domain.Entities.Role.Roles)null); // Simulamos que no se encuentra el rol.

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _handler.Handle(new GetRolesByNameQuery(roleName), CancellationToken.None));
        }

        
        [Fact]
        public async Task Handle_ShouldThrowException_WhenRepositoryFails()
        {
            // Arrange
            var roleName = "Admin";

            _rolesRepositoryMock
                .Setup(repo => repo.GetRolesByNameQuery(roleName))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _handler.Handle(new GetRolesByNameQuery(roleName), CancellationToken.None));
        }
    }
}
