using AutoMapper;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Application.Handlers.Roles.Queries;
using UserMs.Application.Queries.Roles;
using UserMs.Commoon.Dtos.Users.Response.Role;
using UserMs.Core.Repositories.RolesRepo;
using UserMs.Domain.Entities.Role.ValueObjects;
using Xunit;

namespace UserMs.Test.Application.Handler.Queries.Roles
{
    public class GetRolesAllQueryHandlerTests
    {
        private readonly Mock<IRolesRepository> _rolesRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly GetRolesAllQueryHandler _handler;

        public GetRolesAllQueryHandlerTests()
        {
            _rolesRepositoryMock = new Mock<IRolesRepository>();
            _mapperMock = new Mock<IMapper>();

            _handler = new GetRolesAllQueryHandler(
                _rolesRepositoryMock.Object,
                _mapperMock.Object
            );
        }

        [Fact]
        public async Task Handle_ShouldReturnMappedRoles_WhenRolesExist()
        {
            // Arrange
            var admin = RoleName.Create("Admin");
            var user = RoleName.Create("User");
            var roles = new List<Domain.Entities.Role.Roles>
        {
            new Domain.Entities.Role.Roles { RoleId = Guid.NewGuid(), RoleName = admin},
            new Domain.Entities.Role.Roles { RoleId = Guid.NewGuid(), RoleName = user }
        };
            var expectedDtos = new List<GetRoleDto>
        {
            new GetRoleDto { RoleId = roles[0].RoleId, RoleName = roles[0].RoleName.Value },
            new GetRoleDto { RoleId = roles[1].RoleId, RoleName = roles[1].RoleName.Value }
        };

            _rolesRepositoryMock
                .Setup(repo => repo.GetRolesAllQueryAsync())
                .ReturnsAsync(roles);

            _mapperMock
                .Setup(mapper => mapper.Map<List<GetRoleDto>>(roles))
                .Returns(expectedDtos);

            // Act
            var result = await _handler.Handle(new GetRolesAllQuery(), CancellationToken.None);

            // Assert
            Assert.Equal(expectedDtos.Count, result.Count);
            Assert.Equal(expectedDtos[0].RoleName, result[0].RoleName);
            Assert.Equal(expectedDtos[1].RoleName, result[1].RoleName);
        }

   
        [Fact]
        public async Task Handle_ShouldReturnEmptyList_WhenNoRolesFound()
        {
            // Arrange
            _rolesRepositoryMock
                .Setup(repo => repo.GetRolesAllQueryAsync())
                .ReturnsAsync(new List<Domain.Entities.Role.Roles>());

            // Act
            var result = await _handler.Handle(new GetRolesAllQuery(), CancellationToken.None);

            // Assert
            Assert.Empty(result);
        }

        
        [Fact]
        public async Task Handle_ShouldReturnEmptyList_WhenExceptionOccurs()
        {
            // Arrange
            _rolesRepositoryMock
                .Setup(repo => repo.GetRolesAllQueryAsync())
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _handler.Handle(new GetRolesAllQuery(), CancellationToken.None);

            // Assert
            Assert.Empty(result);
        }
    }

}
