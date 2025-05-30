using AutoMapper;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Application.Handlers.Permission;
using UserMs.Application.Queries.Permission;
using UserMs.Commoon.Dtos.Users.Response.Permission;
using UserMs.Core.Repositories.PermissionRepo;
using UserMs.Domain.Entities.Permission;
using UserMs.Domain.Entities.Permission.ValueObjects;
using Xunit;

namespace UserMs.Test.Application.Handler.Queries.Permission
{
    public class GetPermissionAllQueryHandlerTests
    {
        private readonly Mock<IPermissionRepositoryMongo> _permissionRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly GetPermissionAllQueryHandler _handler;

        public GetPermissionAllQueryHandlerTests()
        {
            _permissionRepositoryMock = new Mock<IPermissionRepositoryMongo>();
            _mapperMock = new Mock<IMapper>();

            _handler = new GetPermissionAllQueryHandler(
                _permissionRepositoryMock.Object,
                _mapperMock.Object
            );
        }


        [Fact]
        public async Task Handle_ShouldReturnMappedPermissions_WhenPermissionsExist()
        {
            // Arrange
            var read = PermissionName.Create("Read");
            var write = PermissionName.Create("Write");
            var permissions = new List<Permissions>
            {
                new Permissions(Guid.NewGuid(), read),
                new Permissions(Guid.NewGuid(), write)
            };
            var expectedDtos = new List<GetPermissionDto>
            {
                new GetPermissionDto
                {
                    PermissionId = permissions[0].PermissionId, PermissionName = permissions[0].PermissionName.Value
                },
                new GetPermissionDto
                    { PermissionId = permissions[1].PermissionId, PermissionName = permissions[1].PermissionName.Value }
            };

            _permissionRepositoryMock
                .Setup(repo => repo.GetPermissionAllQueryAsync())
                .ReturnsAsync(permissions);

            _mapperMock
                .Setup(mapper => mapper.Map<List<GetPermissionDto>>(permissions))
                .Returns(expectedDtos);

            // Act
            var result = await _handler.Handle(new GetPermissionAllQuery(), CancellationToken.None);

            // Assert
            Assert.Equal(expectedDtos.Count, result.Count);
            Assert.Equal(expectedDtos[0].PermissionName, result[0].PermissionName);
            Assert.Equal(expectedDtos[1].PermissionName, result[1].PermissionName);
        }

     
        [Fact]
        public async Task Handle_ShouldReturnEmptyList_WhenNoPermissionsFound()
        {
            // Arrange
            _permissionRepositoryMock
                .Setup(repo => repo.GetPermissionAllQueryAsync())
                .ReturnsAsync(new List<Permissions>());

            // Act
            var result = await _handler.Handle(new GetPermissionAllQuery(), CancellationToken.None);

            // Assert
            Assert.Empty(result);
        }

     
        [Fact]
        public async Task Handle_ShouldReturnEmptyList_WhenRepositoryFails()
        {
            // Arrange
            _permissionRepositoryMock
                .Setup(repo => repo.GetPermissionAllQueryAsync())
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _handler.Handle(new GetPermissionAllQuery(), CancellationToken.None);

            // Assert
            Assert.Empty(result);
        }

    }
}
