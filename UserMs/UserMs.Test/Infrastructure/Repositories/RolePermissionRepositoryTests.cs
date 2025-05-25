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

namespace UserMs.Test.Infrastructure.Repositories
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

            //_repository = new RolePermissionRepository(_dbContextMock.Object, _mongoContextMock.Object, _mapperMock.Object);
        }

        // 🔹 Validación del Constructor
        [Fact]
        public void Constructor_ShouldThrowException_WhenDbContextIsNull()
        {
           // Assert.Throws<ArgumentNullException>(() => new RolePermissionRepository(null, _mongoContextMock.Object, _mapperMock.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowException_WhenMongoContextIsNull()
        {
           // Assert.Throws<ArgumentNullException>(() => new RolePermissionRepository(_dbContextMock.Object, null, _mapperMock.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowException_WhenMapperIsNull()
        {
           // Assert.Throws<ArgumentNullException>(() => new RolePermissionRepository(_dbContextMock.Object, _mongoContextMock.Object, null));
        }

        // 🔹 Prueba de `AddAsync()`
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

        // 🔹 Prueba de `GetRolesPermissionByRoleQuery()`
        [Fact]
        public async Task GetRolesPermissionByRoleQuery_ShouldReturnRolePermission_WhenFound()
        {
            var roleName = "Admin";
            var rolePermissionId = RolePermissionId.Create(Guid.NewGuid());
            var rolId = RoleId.Create(Guid.NewGuid());
            var permissionId = PermissionId.Create(Guid.NewGuid());
            var rolePermissionsDto = new GetRolePermissionDto {RoleId = rolId.Value,RoleName = "Admin", PermissionName = "Read Access",PermissionId = permissionId.Value };
            var rolePermissionsEntity = new RolePermissions (rolePermissionId, rolId, permissionId);

            var mockCursor = new Mock<IAsyncCursor<GetRolePermissionDto>>();
            mockCursor.SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)
                .ReturnsAsync(false);
            mockCursor.SetupGet(c => c.Current).Returns(new List<GetRolePermissionDto> { rolePermissionsDto });

            _mongoCollectionMock.Setup(c => c.FindAsync(
                    It.IsAny<FilterDefinition<RolePermissions>>(),
                    It.IsAny<FindOptions<RolePermissions, GetRolePermissionDto>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockCursor.Object);

            _mapperMock.Setup(m => m.Map<RolePermissions>(rolePermissionsDto)).Returns(rolePermissionsEntity);

          /*  var result = await _repository.GetRolesPermissionByRoleQuery(roleName);

            Assert.NotNull(result);
            Assert.Equal(rolId.Value, result.RoleId.Value);
            Assert.Equal(permissionId.Value, result.PermissionId.Value);*/
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

        [Fact]
        public async Task GetByRoleAndPermissionAsync_ShouldReturnNull_WhenNoMatchingRoleAndPermissionFound()
        {
            var roleId = RoleId.Create(Guid.NewGuid());
            var permissionId = PermissionId.Create(Guid.NewGuid());

            var mockCursor = new Mock<IAsyncCursor<GetRolePermissionDto>>();
            mockCursor.SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(false); // 🔹 No hay registros encontrados
            mockCursor.SetupGet(c => c.Current).Returns(new List<GetRolePermissionDto>());

            _mongoCollectionMock.Setup(c => c.FindAsync(
                    It.IsAny<FilterDefinition<RolePermissions>>(),
                    It.IsAny<FindOptions<RolePermissions, GetRolePermissionDto>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockCursor.Object);

            _mapperMock.Setup(m => m.Map<RolePermissions>(It.IsAny<GetRolePermissionDto>()))
                .Returns((RolePermissions)null); // 🔹 Simula que no hay datos

           /* var result = await _repository.GetByRoleAndPermissionAsync(roleId.Value, permissionId.Value);

            Assert.Null(result); // ✅ Validación final*/
        }

        [Fact]
        public async Task GetByRoleAndPermissionAsync_ShouldReturnRolePermission_WhenFound()
        {
            var roleId = RoleId.Create(Guid.NewGuid());
            var permissionId = PermissionId.Create(Guid.NewGuid());
            var rolePermissionDto = new GetRolePermissionDto { RoleId = roleId.Value, PermissionId = permissionId.Value, RoleName = "Admin", PermissionName = "Read Access" };
            var rolePermissionEntity = new RolePermissions(rolePermissionId: RolePermissionId.Create(Guid.NewGuid()), roleId, permissionId);

            var mockCursor = new Mock<IAsyncCursor<GetRolePermissionDto>>();
            mockCursor.SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)
                .ReturnsAsync(false);
            mockCursor.SetupGet(c => c.Current).Returns(new List<GetRolePermissionDto> { rolePermissionDto });

            _mongoCollectionMock.Setup(c => c.FindAsync(
                    It.IsAny<FilterDefinition<RolePermissions>>(),
                    It.IsAny<FindOptions<RolePermissions, GetRolePermissionDto>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockCursor.Object);

            _mapperMock.Setup(m => m.Map<RolePermissions>(rolePermissionDto)).Returns(rolePermissionEntity);

          /*  var result = await _repository.GetByRoleAndPermissionAsync(roleId.Value, permissionId.Value);

            Assert.NotNull(result);
            Assert.Equal(roleId.Value, result.RoleId.Value);
            Assert.Equal(permissionId.Value, result.PermissionId.Value);
            Assert.Equal("Admin", rolePermissionDto.RoleName);
            Assert.Equal("Read Access", rolePermissionDto.PermissionName);*/
        }

        [Fact]
        public async Task GetRolesPermissionByIdQuery_ShouldReturnNull_WhenRolePermissionNotFound()
        {
            var rolePermissionId = RolePermissionId.Create(Guid.NewGuid());

            var mockCursor = new Mock<IAsyncCursor<GetRolePermissionDto>>();
            mockCursor.SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(false); // 🔹 No hay registros encontrados
            mockCursor.SetupGet(c => c.Current).Returns(new List<GetRolePermissionDto>());

            _mongoCollectionMock.Setup(c => c.FindAsync(
                    It.IsAny<FilterDefinition<RolePermissions>>(),
                    It.IsAny<FindOptions<RolePermissions, GetRolePermissionDto>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockCursor.Object);

            _mapperMock.Setup(m => m.Map<RolePermissions>(It.IsAny<GetRolePermissionDto>()))
                .Returns((RolePermissions)null); // 🔹 Simula que no hay datos

            /*var result = await _repository.GetRolesPermissionByIdQuery(rolePermissionId.Value);

            Assert.Null(result); // ✅ Validación final*/
        }

        [Fact]
        public async Task GetRolesPermissionByIdQuery_ShouldReturnRolePermissions_WhenFound()
        {
            var rolePermissionId = RolePermissionId.Create(Guid.NewGuid());
            var rolePermissionsDto = new GetRolePermissionDto { RolePermissionId = rolePermissionId.Value, RoleName = "Admin", PermissionName = "Read Access" };
            var rolePermissionsEntity = new RolePermissions(rolePermissionId.Value, RoleId.Create(Guid.NewGuid()), PermissionId.Create(Guid.NewGuid()));

            var mockCursor = new Mock<IAsyncCursor<GetRolePermissionDto>>();
            mockCursor.SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)
                .ReturnsAsync(false);
            mockCursor.SetupGet(c => c.Current).Returns(new List<GetRolePermissionDto> { rolePermissionsDto });

            _mongoCollectionMock.Setup(c => c.FindAsync(
                    It.IsAny<FilterDefinition<RolePermissions>>(),
                    It.IsAny<FindOptions<RolePermissions, GetRolePermissionDto>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockCursor.Object);

            _mapperMock.Setup(m => m.Map<RolePermissions>(rolePermissionsDto)).Returns(rolePermissionsEntity);

          //  var result = await _repository.GetRolesPermissionByIdQuery(rolePermissionId.Value);

           // Assert.NotNull(result);
          //  Assert.Equal(rolePermissionId.Value, result.RolePermissionId.Value);
        }


        [Fact]
        public async Task GetRolesPermissionAsync_ShouldReturnEmptyList_WhenNoRolePermissionsFound()
        {
            var mockCursor = new Mock<IAsyncCursor<GetRolePermissionDto>>();
            mockCursor.SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(false); // 🔹 No hay registros
            mockCursor.SetupGet(c => c.Current).Returns(new List<GetRolePermissionDto>()); // 🔹 Lista vacía

            _mongoCollectionMock.Setup(c => c.FindAsync(
                    It.IsAny<FilterDefinition<RolePermissions>>(),
                    It.IsAny<FindOptions<RolePermissions, GetRolePermissionDto>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockCursor.Object);

            _mapperMock.Setup(m => m.Map<List<RolePermissions>>(It.IsAny<List<GetRolePermissionDto>>()))
                .Returns(new List<RolePermissions>()); // 🔹 Mapeo a lista vacía

           // var result = await _repository.GetRolesPermissionAsync();

           // Assert.NotNull(result);
           // Assert.Empty(result);
           // Assert.IsType<List<RolePermissions>>(result); // ✅ Validación de tipo
        }

        [Fact]
        public async Task GetRolesPermissionAsync_ShouldReturnRolePermissions_WhenRolePermissionsExist()
        {
            var rolePermissionsDtos = new List<GetRolePermissionDto>
            {
                new() { RolePermissionId = Guid.NewGuid(), RoleName = "Admin", PermissionName = "Read Access" },
                new() { RolePermissionId = Guid.NewGuid(), RoleName = "User", PermissionName = "Write Access" }
            };

            var rolePermissionsEntities = new List<RolePermissions>
            {
                new RolePermissions(RolePermissionId.Create(rolePermissionsDtos[0].RolePermissionId), RoleId.Create(Guid.NewGuid()), PermissionId.Create(Guid.NewGuid())),
                new RolePermissions(RolePermissionId.Create(rolePermissionsDtos[1].RolePermissionId), RoleId.Create(Guid.NewGuid()), PermissionId.Create(Guid.NewGuid()))
            };

            var mockCursor = new Mock<IAsyncCursor<GetRolePermissionDto>>();
            mockCursor.SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)
                .ReturnsAsync(false);
            mockCursor.SetupGet(c => c.Current).Returns(rolePermissionsDtos);

            _mongoCollectionMock.Setup(c => c.FindAsync(
                    It.IsAny<FilterDefinition<RolePermissions>>(),
                    It.IsAny<FindOptions<RolePermissions, GetRolePermissionDto>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockCursor.Object);

            _mapperMock.Setup(m => m.Map<List<RolePermissions>>(rolePermissionsDtos)).Returns(rolePermissionsEntities);

          //  var result = await _repository.GetRolesPermissionAsync();

          //  Assert.NotNull(result);
          //  Assert.Equal(2, result.Count);
          //  Assert.IsType<List<RolePermissions>>(result);
        }
    }
}
