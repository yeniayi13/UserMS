using AutoMapper;
using MongoDB.Driver;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Commoon.Dtos.Users.Response.Role;
using UserMs.Core.Database;
using UserMs.Domain.Entities.Role.ValueObjects;
using UserMs.Domain.Entities.Role;
using UserMs.Infrastructure.Repositories.RolesRepo;
using Xunit;

namespace UserMs.Test.Infrastructure.Repositories.Mongo
{
    public class RoleRepositoryMongoTests
    {
    
        private Mock<IUserDbContextMongo> _mongoContextMock;
        private Mock<IMongoCollection<Roles>> _mongoCollectionMock;
        private Mock<IMapper> _mapperMock;
        private RoleRepository _repository;

        public RoleRepositoryMongoTests()
        {
            
            _mongoContextMock = new Mock<IUserDbContextMongo>();
            _mongoCollectionMock = new Mock<IMongoCollection<Roles>>();
            _mapperMock = new Mock<IMapper>();

            var mockDatabase = new Mock<IMongoDatabase>();
            mockDatabase.Setup(db => db.GetCollection<Roles>("Roles", null))
                .Returns(_mongoCollectionMock.Object);

            _mongoContextMock.Setup(m => m.Database).Returns(mockDatabase.Object);

            _repository = new RoleRepository(_mongoContextMock.Object, _mapperMock.Object);


        } // 🔹 Validación del Constructor


            [Fact]
            public void Constructor_ShouldThrowException_WhenMongoContextIsNull()
            {
                Assert.Throws<ArgumentNullException>(() => new RoleRepository(null, _mapperMock.Object));
            }

            [Fact]
            public void Constructor_ShouldThrowException_WhenMongoDatabaseIsNull()
            {
                _mongoContextMock.Setup(m => m.Database).Returns((IMongoDatabase)null);
                Assert.Throws<ArgumentNullException>(() =>
                    new RoleRepository(_mongoContextMock.Object, _mapperMock.Object));
            }

            [Fact]
            public void Constructor_ShouldThrowException_WhenMapperIsNull()
            {
                Assert.Throws<ArgumentNullException>(() => new RoleRepository(_mongoContextMock.Object, null));
            }

            // 🔹 Validación de `GetRolesAllQueryAsync()`
            [Fact]
            public async Task GetRolesAllQueryAsync_ShouldReturnEmptyList_WhenNoRolesFound()
            {
                var mockCursor = new Mock<IAsyncCursor<GetRoleDto>>();
                mockCursor.SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
                    .ReturnsAsync(false);
                mockCursor.SetupGet(c => c.Current).Returns(new List<GetRoleDto>());

                _mongoCollectionMock.Setup(c => c.FindAsync(
                        It.IsAny<FilterDefinition<Roles>>(),
                        It.IsAny<FindOptions<Roles, GetRoleDto>>(),
                        It.IsAny<CancellationToken>()))
                    .ReturnsAsync(mockCursor.Object);

                _mapperMock.Setup(m => m.Map<List<Roles>>(It.IsAny<List<GetRoleDto>>()))
                    .Returns(new List<Roles>());

                var result = await _repository.GetRolesAllQueryAsync();

                Assert.NotNull(result);
                Assert.Empty(result);
            }

            [Fact]
            public async Task GetRolesAllQueryAsync_ShouldReturnRolesList_WhenRolesExist()
            {

                var roleName = RoleName.Create("Admin");
                var roleName2 = RoleName.Create("User");
                var roleDtos = new List<GetRoleDto>
                {
                    new() { RoleId = Guid.NewGuid(), RoleName = "Admin" },
                    new() { RoleId = Guid.NewGuid(), RoleName = "User" }
                };

                var roleEntities = new List<Roles>
                {
                    new Roles { RoleId = roleDtos[0].RoleId, RoleName = roleName },
                    new Roles { RoleId = roleDtos[1].RoleId, RoleName = roleName2 }
                };

                var mockCursor = new Mock<IAsyncCursor<GetRoleDto>>();
                mockCursor.SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
                    .ReturnsAsync(true)
                    .ReturnsAsync(false);
                mockCursor.SetupGet(c => c.Current).Returns(roleDtos);

                _mongoCollectionMock.Setup(c => c.FindAsync(
                        It.IsAny<FilterDefinition<Roles>>(),
                        It.IsAny<FindOptions<Roles, GetRoleDto>>(),
                        It.IsAny<CancellationToken>()))
                    .ReturnsAsync(mockCursor.Object);

                _mapperMock.Setup(m => m.Map<List<Roles>>(roleDtos)).Returns(roleEntities);

                var result = await _repository.GetRolesAllQueryAsync();

                Assert.NotNull(result);
                Assert.Equal(2, result.Count);
                Assert.Equal("Admin", result[0].RoleName.Value);
                Assert.Equal("User", result[1].RoleName.Value);
            }

            [Fact]
            public async Task GetRolesByIdQuery_ShouldReturnNull_WhenRoleNotFound()
            {
                var roleId = Guid.NewGuid();

                var mockCursor = new Mock<IAsyncCursor<GetRoleDto>>();
                mockCursor.SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
                    .ReturnsAsync(false);
                mockCursor.SetupGet(c => c.Current)
                    .Returns(new List<GetRoleDto>()); // 🔹 Asegurar que `Current` no es `null`

                _mongoCollectionMock.Setup(c => c.FindAsync(
                        It.IsAny<FilterDefinition<Roles>>(),
                        It.IsAny<FindOptions<Roles, GetRoleDto>>(),
                        It.IsAny<CancellationToken>()))
                    .ReturnsAsync(mockCursor.Object); // 🔹 Evita retorno `null`

                var result = await _repository.GetRolesByIdQuery(roleId);

                Assert.Null(result);
            }

            [Fact]
            public async Task GetRolesByNameQuery_ShouldReturnNull_WhenRoleNotFound()
            {
                var roleName = "NonExistentRole";

                var mockCursor = new Mock<IAsyncCursor<GetRoleDto>>();
                mockCursor.SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
                    .ReturnsAsync(false); // 🔹 No hay registros encontrados
                mockCursor.SetupGet(c => c.Current).Returns(new List<GetRoleDto>());

                _mongoCollectionMock.Setup(c => c.FindAsync(
                        It.IsAny<FilterDefinition<Roles>>(),
                        It.IsAny<FindOptions<Roles, GetRoleDto>>(),
                        It.IsAny<CancellationToken>()))
                    .ReturnsAsync(mockCursor.Object);

                _mapperMock.Setup(m => m.Map<Roles>(It.IsAny<GetRoleDto>()))
                    .Returns((Roles)null); // 🔹 Simula que no hay datos

                var result = await _repository.GetRolesByNameQuery(roleName);

                Assert.Null(result); // ✅ Validación final
            }

            [Fact]
            public async Task GetRolesByNameQuery_ShouldReturnRole_WhenFound()
            {
                var roleName = RoleName.Create("Admin");
                var roleDto = new GetRoleDto { RoleId = Guid.NewGuid(), RoleName = "Admin" };
                var roleEntity = new Roles { RoleId = roleDto.RoleId, RoleName = roleName };

                var mockCursor = new Mock<IAsyncCursor<GetRoleDto>>();
                mockCursor.SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
                    .ReturnsAsync(true)
                    .ReturnsAsync(false);
                mockCursor.SetupGet(c => c.Current).Returns(new List<GetRoleDto> { roleDto });

                _mongoCollectionMock.Setup(c => c.FindAsync(
                        It.IsAny<FilterDefinition<Roles>>(),
                        It.IsAny<FindOptions<Roles, GetRoleDto>>(),
                        It.IsAny<CancellationToken>()))
                    .ReturnsAsync(mockCursor.Object);

                _mapperMock.Setup(m => m.Map<Roles>(roleDto)).Returns(roleEntity);

                var result = await _repository.GetRolesByNameQuery(roleName.Value);

                Assert.NotNull(result);
                Assert.Equal(roleDto.RoleId, result.RoleId.Value);
                Assert.Equal("Admin", result.RoleName.Value);
            }

        
    }
}
