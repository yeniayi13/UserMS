using AutoMapper;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using MongoDB.Driver;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Commoon.Dtos.Users.Response.Role_Permission;
using UserMs.Core.Database;
using UserMs.Domain.Entities.Permission.ValueObjects;
using UserMs.Domain.Entities.Role_Permission.ValueObjects;
using UserMs.Domain.Entities.Role_Permission;
using UserMs.Domain.Entities.Role.ValueObjects;
using UserMs.Infrastructure.Repositories.Roles_Permission;
using Xunit;

namespace UserMs.Test.Infrastructure.Repositories.Postgres
{
    public class RolePermissionRepositoryTests
    {
        private Mock<IUserDbContext> _dbContextMock;
        private Mock<IUserDbContextMongo> _mongoContextMock;
        private Mock<IMongoCollection<RolePermissions>> _mongoCollectionMock;
        private Mock<IMapper> _mapperMock;
        private RolePermissionRepository _repository;

        public RolePermissionRepositoryTests()
        {
            _dbContextMock = new Mock<IUserDbContext>();
            _mongoContextMock = new Mock<IUserDbContextMongo>();
            _mongoCollectionMock = new Mock<IMongoCollection<RolePermissions>>();
            _mapperMock = new Mock<IMapper>();

            var mockDatabase = new Mock<IMongoDatabase>();
            mockDatabase.Setup(db => db.GetCollection<RolePermissions>("RolePermissions", null))
                .Returns(_mongoCollectionMock.Object);

            _mongoContextMock.Setup(m => m.Database).Returns(mockDatabase.Object);

            _repository = new RolePermissionRepository(_dbContextMock.Object, _mapperMock.Object);
        }

        // 🔹 Validación del Constructor
        [Fact]
        public void Constructor_ShouldThrowException_WhenDbContextIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new RolePermissionRepository(null, _mapperMock.Object));
        }

   
        [Fact]
        public void Constructor_ShouldThrowException_WhenMapperIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new RolePermissionRepository(_dbContextMock.Object,  null));
        }

        [Fact]
        public async Task AddAsync_ShouldAddRolePermissionSuccessfully()
        {
            var rolePermissionId = RolePermissionId.Create(Guid.NewGuid());
            var rolId = RoleId.Create(Guid.NewGuid());
            var permissionId = PermissionId.Create(Guid.NewGuid());
            var rolePermission = new RolePermissions (rolePermissionId, rolId, permissionId );

            _dbContextMock.Setup(db => db.RolePermissions.AddAsync(rolePermission, default))
                .ReturnsAsync((EntityEntry<RolePermissions>)null);

            _dbContextMock.Setup(db => db.SaveEfContextChanges(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            await _repository.AddAsync(rolePermission);

            _dbContextMock.Verify(db => db.RolePermissions.AddAsync(rolePermission, default), Times.Once);
            _dbContextMock.Verify(db => db.SaveEfContextChanges(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        // 🔹 Prueba de `DeleteRolePermissionAsync()`
        [Fact]
        public async Task DeleteRolePermissionAsync_ShouldDeleteRolePermissionSuccessfully()
        {
            var rolePermissionId =  RolePermissionId.Create(Guid.NewGuid());
            var rolId = RoleId.Create(Guid.NewGuid());
            var permissionId = PermissionId.Create(Guid.NewGuid());
            var existingRolePermission =new   RolePermissions ( rolePermissionId, rolId, permissionId);

            _dbContextMock.Setup(db => db.RolePermissions.FindAsync(rolePermissionId))
                .ReturnsAsync(existingRolePermission);

            _dbContextMock.Setup(db => db.SaveEfContextChanges(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await _repository.DeleteRolePermissionAsync(rolePermissionId);

            Assert.NotNull(result);
            Assert.Equal(rolePermissionId.Value, result.RolePermissionId.Value);

            _dbContextMock.Verify(db => db.RolePermissions.FindAsync(rolePermissionId), Times.Once);
            _dbContextMock.Verify(db => db.SaveEfContextChanges(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

     

        [Fact]
        public async Task UpdateRolePermissionAsync_ShouldUpdateRolePermissionSuccessfully()
        {
            var rolePermissionId =  RolePermissionId.Create(Guid.NewGuid());
            var roleId = RoleId.Create(Guid.NewGuid());
            var permissionId = PermissionId.Create(Guid.NewGuid());
            var rolePermission = new RolePermissions
            (
                rolePermissionId.Value,
                 roleId,
                permissionId
            );

            // Simular `Update()`
            _dbContextMock.Setup(db => db.RolePermissions.Update(rolePermission));

            // Simular `SaveEfContextChanges`
            _dbContextMock.Setup(db => db.SaveEfContextChanges(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await _repository.UpdateRolePermissionAsync(rolePermissionId, rolePermission);

            // ✅ Validaciones
            Assert.NotNull(result);
            Assert.Equal(rolePermissionId.Value, result.RolePermissionId.Value);
            Assert.Equal(rolePermission.RoleId, result.RoleId);
            Assert.Equal(rolePermission.PermissionId, result.PermissionId);

            // ✅ Verificaciones
            _dbContextMock.Verify(db => db.RolePermissions.Update(rolePermission), Times.Once);
            _dbContextMock.Verify(db => db.SaveEfContextChanges(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }


       
    }
}
