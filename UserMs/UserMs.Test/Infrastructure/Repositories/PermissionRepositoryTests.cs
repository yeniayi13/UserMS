using AutoMapper;
using MongoDB.Driver;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Commoon.Dtos.Users.Response.Permission;
using UserMs.Core.Database;
using UserMs.Domain.Entities.Permission;
using UserMs.Domain.Entities.Permission.ValueObjects;
using UserMs.Infrastructure.Repositories.PermissionsRepo;
using Xunit;

namespace UserMs.Test.Infrastructure.Repositories
{
    public class PermissionRepositoryTests
    {
        private Mock<IUserDbContext> _dbContextMock;
        private Mock<IUserDbContextMongo> _mongoContextMock;
        private Mock<IMongoCollection<Permissions>> _mongoCollectionMock;
        private Mock<IMapper> _mapperMock;
        private PermissionRepository _repository;

        public PermissionRepositoryTests()
        {
            _dbContextMock = new Mock<IUserDbContext>();
            _mongoContextMock = new Mock<IUserDbContextMongo>();
            _mongoCollectionMock = new Mock<IMongoCollection<Permissions>>();
            _mapperMock = new Mock<IMapper>();

            var mockDatabase = new Mock<IMongoDatabase>();
            mockDatabase.Setup(db => db.GetCollection<Permissions>("Permissions", null))
                .Returns(_mongoCollectionMock.Object);

            _mongoContextMock.Setup(m => m.Database).Returns(mockDatabase.Object);

            _repository = new PermissionRepository(_dbContextMock.Object, _mongoContextMock.Object, _mapperMock.Object);
        }

        [Fact]
        public void Constructor_ShouldThrowException_WhenDbContextIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new PermissionRepository(null, _mongoContextMock.Object, _mapperMock.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowException_WhenMongoContextIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new PermissionRepository(_dbContextMock.Object, null, _mapperMock.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowException_WhenMongoDatabaseIsNull()
        {
            _mongoContextMock.Setup(m => m.Database).Returns((IMongoDatabase)null);
            Assert.Throws<ArgumentNullException>(() => new PermissionRepository(_dbContextMock.Object, _mongoContextMock.Object, _mapperMock.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowException_WhenMapperIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new PermissionRepository(_dbContextMock.Object, _mongoContextMock.Object, null));
        }

        [Fact]
        public async Task GetPermissionAllQueryAsync_ShouldReturnEmptyList_WhenNoPermissionsFound()
        {
            var mockCursor = new Mock<IAsyncCursor<GetPermissionDto>>();
            mockCursor.SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(false); // 🔹 Simula que no hay registros en la consulta
            mockCursor.SetupGet(c => c.Current).Returns(new List<GetPermissionDto>());

            _mongoCollectionMock.Setup(c => c.FindAsync(
                    It.IsAny<FilterDefinition<Permissions>>(),
                    It.IsAny<FindOptions<Permissions, GetPermissionDto>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockCursor.Object);

            _mapperMock.Setup(m => m.Map<List<Permissions>>(It.IsAny<List<GetPermissionDto>>()))
                .Returns(new List<Permissions>());

            var result = await _repository.GetPermissionAllQueryAsync();

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetPermissionAllQueryAsync_ShouldReturnPermissions_WhenFound()
        {
            var permissionsDtos = new List<GetPermissionDto>
            {
                new() { PermissionId = Guid.NewGuid(), PermissionName = "Read" },
                new() { PermissionId = Guid.NewGuid(), PermissionName = "Write" }
            };

            var permissionsEntities = new List<Permissions>
            {
                new Permissions ( PermissionId.Create (permissionsDtos[0].PermissionId), PermissionName.Create("Read") ),
                new Permissions (PermissionId.Create (permissionsDtos[1].PermissionId), PermissionName.Create("Write"))
            };

            var mockCursor = new Mock<IAsyncCursor<GetPermissionDto>>();
            mockCursor.SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)
                .ReturnsAsync(false);
            mockCursor.SetupGet(c => c.Current).Returns(permissionsDtos);

            _mongoCollectionMock.Setup(c => c.FindAsync(
                    It.IsAny<FilterDefinition<Permissions>>(),
                    It.IsAny<FindOptions<Permissions, GetPermissionDto>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockCursor.Object);

            _mapperMock.Setup(m => m.Map<List<Permissions>>(permissionsDtos)).Returns(permissionsEntities);

            var result = await _repository.GetPermissionAllQueryAsync();

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal(permissionsDtos[0].PermissionId, result[0].PermissionId.Value);
            Assert.Equal("Read", result[0].PermissionName.Value);
            Assert.Equal(permissionsDtos[1].PermissionId, result[1].PermissionId.Value);
            Assert.Equal("Write", result[1].PermissionName.Value);
        }
        [Fact]
        public async Task GetPermissionByIdAsync_ShouldReturnNull_WhenPermissionNotFound()
        {
            var permissionId = Guid.NewGuid();

            var mockCursor = new Mock<IAsyncCursor<GetPermissionDto>>();
            mockCursor.SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);
            mockCursor.SetupGet(c => c.Current).Returns(new List<GetPermissionDto>()); // 🔹 Asegurar que Current no es `null`

            _mongoCollectionMock.Setup(c => c.FindAsync(
                    It.IsAny<FilterDefinition<Permissions>>(),
                    It.IsAny<FindOptions<Permissions, GetPermissionDto>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockCursor.Object);

            _mapperMock.Setup(m => m.Map<Permissions>(It.IsAny<GetPermissionDto>()))
                .Returns((Permissions)null); // 🔹 Simula que no hay permiso encontrado

            var result = await _repository.GetPermissionByIdAsync(permissionId);

            Assert.Null(result); // ✅ Validación final
        }

        [Fact]
        public async Task GetPermissionByIdAsync_ShouldReturnPermission_WhenFound()
        {
            var permissionId = Guid.NewGuid();
            var permissionsDto = new GetPermissionDto { PermissionId = permissionId, PermissionName = "Execute" };
            var permissionEntity = new Permissions ( PermissionId.Create(permissionId), PermissionName.Create("Execute") );

            var mockCursor = new Mock<IAsyncCursor<GetPermissionDto>>();
            mockCursor.SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)
                .ReturnsAsync(false);
            mockCursor.SetupGet(c => c.Current).Returns(new List<GetPermissionDto> { permissionsDto });

            _mongoCollectionMock.Setup(c => c.FindAsync(
                    It.IsAny<FilterDefinition<Permissions>>(),
                    It.IsAny<FindOptions<Permissions, GetPermissionDto>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockCursor.Object);

            _mapperMock.Setup(m => m.Map<Permissions>(permissionsDto)).Returns(permissionEntity);

            var result = await _repository.GetPermissionByIdAsync(permissionId);

            Assert.NotNull(result);
            Assert.Equal("Execute", result.PermissionName.Value);
            //Assert.Equal("Allows execution", result.PermissionDescription);
        }
    }
}
