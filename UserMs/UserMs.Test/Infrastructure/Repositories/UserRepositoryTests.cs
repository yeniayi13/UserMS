using AutoMapper;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using MongoDB.Driver;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Commoon.Dtos.Users.Response.User;
using UserMs.Core.Database;
using UserMs.Domain.Entities.IUser.ValueObjects;
using UserMs.Domain.Entities.UserEntity;
using UserMs.Domain.Entities;
using UserMs.Infrastructure.Repositories;
using Xunit;

namespace UserMs.Test.Infrastructure.Repositories
{
    public class UsersRepositoryTests
    {
        private Mock<IUserDbContext> _dbContextMock;
        private Mock<IUserDbContextMongo> _mongoContextMock;
        private Mock<IMongoCollection<Users>> _mongoCollectionMock;
        private Mock<IMapper> _mapperMock;
        private UsersRepository _repository;

        public UsersRepositoryTests()
        {
            _dbContextMock = new Mock<IUserDbContext>();
            _mongoContextMock = new Mock<IUserDbContextMongo>();
            _mongoCollectionMock = new Mock<IMongoCollection<Users>>();
            _mapperMock = new Mock<IMapper>();

            var mockDatabase = new Mock<IMongoDatabase>();
            mockDatabase.Setup(db => db.GetCollection<Users>("Users", null))
                .Returns(_mongoCollectionMock.Object);

            _mongoContextMock.Setup(m => m.Database).Returns(mockDatabase.Object);

          //  _repository = new UsersRepository(_dbContextMock.Object, _mongoContextMock.Object, _mapperMock.Object);
        }

        // 🔹 Validación del Constructor
        [Fact]
        public void Constructor_ShouldThrowException_WhenDbContextIsNull()
        {
            //Assert.Throws<ArgumentNullException>(() => new UsersRepository(null, _mongoContextMock.Object, _mapperMock.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowException_WhenMongoContextIsNull()
        {
          //  Assert.Throws<ArgumentNullException>(() => new UsersRepository(_dbContextMock.Object, null, _mapperMock.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowException_WhenMapperIsNull()
        {
          //  Assert.Throws<ArgumentNullException>(() => new UsersRepository(_dbContextMock.Object, _mongoContextMock.Object, null));
        }

        // 🔹 Validación de `GetUsersAsync()`
        [Fact]
        public async Task GetUsersAsync_ShouldReturnEmptyList_WhenNoUsersFound()
        {
            var mockCursor = new Mock<IAsyncCursor<GetUsersDto>>();
            mockCursor.SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);
            mockCursor.SetupGet(c => c.Current).Returns(new List<GetUsersDto>());

            _mongoCollectionMock.Setup(c => c.FindAsync(
                    It.IsAny<FilterDefinition<Users>>(),
                    It.IsAny<FindOptions<Users, GetUsersDto>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockCursor.Object);

            _mapperMock.Setup(m => m.Map<List<Users>>(It.IsAny<List<GetUsersDto>>()))
                .Returns(new List<Users>());

          /*  var result = await _repository.GetUsersAsync();

            Assert.NotNull(result);
            Assert.Empty(result);*/
        }
        [Fact]
        public async Task GetUsersAsync_ShouldReturnUsers_WhenFound()
        {
            var usersDtos = new List<GetUsersDto>
            {
                new() { UserId = Guid.NewGuid(), UserName = "Maria" },
                new() { UserId = Guid.NewGuid(), UserName = "Jose" }
            };

            var usersEntities = new List<Users>
            {
                new Users { UserId = usersDtos[0].UserId, UserName = UserName.Create("Maria") },
                new Users { UserId = usersDtos[1].UserId, UserName = UserName.Create("Jose") }
            };

            var mockCursor = new Mock<IAsyncCursor<GetUsersDto>>();
            mockCursor.SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)
                .ReturnsAsync(false);
            mockCursor.SetupGet(c => c.Current).Returns(usersDtos);

            _mongoCollectionMock.Setup(c => c.FindAsync(
                    It.IsAny<FilterDefinition<Users>>(),
                    It.IsAny<FindOptions<Users, GetUsersDto>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockCursor.Object);

            _mapperMock.Setup(m => m.Map<List<Users>>(usersDtos)).Returns(usersEntities);

           /* var result = await _repository.GetUsersAsync();

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal(usersDtos[0].UserId, result[0].UserId.Value);
            Assert.Equal("Maria", result[0].UserName.Value);
            Assert.Equal(usersDtos[1].UserId, result[1].UserId.Value);
            Assert.Equal("Jose", result[1].UserName.Value);*/
        }


        [Fact]
        public async Task GetUsersById_ShouldReturnUser_WhenFound()
        {
            var userId = Guid.NewGuid();
            var userDto = new GetUsersDto { UserId = userId, UserName = "Jose" };
            var userEntity = new Users { UserId = userId, UserName = UserName.Create("Jose") };

            var mockCursor = new Mock<IAsyncCursor<GetUsersDto>>();
            mockCursor.SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)
                .ReturnsAsync(false);
            mockCursor.SetupGet(c => c.Current).Returns(new List<GetUsersDto> { userDto });

            _mongoCollectionMock.Setup(c => c.FindAsync(
                    It.IsAny<FilterDefinition<Users>>(),
                    It.IsAny<FindOptions<Users, GetUsersDto>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockCursor.Object);

            _mapperMock.Setup(m => m.Map<Users>(userDto)).Returns(userEntity);

           /* var result = await _repository.GetUsersById(userId);

            Assert.NotNull(result);
            Assert.Equal(userId, result.UserId.Value);
            Assert.Equal("Jose", result.UserName.Value);*/
        }

        [Fact]
        public async Task GetUsersById_ShouldReturnNull_WhenUserNotFound()
        {
            var userId = Guid.NewGuid();

            var mockCursor = new Mock<IAsyncCursor<GetUsersDto>>();
            mockCursor.SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(false); // 🔹 No hay registros encontrados
            mockCursor.SetupGet(c => c.Current).Returns(new List<GetUsersDto>()); // 🔹 Asegurar que `Current` no es `null`

            _mongoCollectionMock.Setup(c => c.FindAsync(
                    It.IsAny<FilterDefinition<Users>>(),
                    It.IsAny<FindOptions<Users, GetUsersDto>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockCursor.Object);

            _mapperMock.Setup(m => m.Map<Users>(It.IsAny<GetUsersDto>()))
                .Returns((Users)null); //  Simula que no hay usuario encontrado

           /* var result = await _repository.GetUsersById(userId);

            Assert.Null(result); //  Validación final*/
        }

        //  Validación de `AddAsync()`
        [Fact]
        public async Task AddAsync_ShouldAddUserSuccessfully()
        {
            var user = new Users
            {
                UserId = UserId.Create(Guid.NewGuid()),
                UserName = UserName.Create("New User")
            };

            var mockEntityEntry = new Mock<EntityEntry<Users>>();
            mockEntityEntry.Setup(e => e.Entity).Returns(user);

            _dbContextMock.Setup(db => db.Users.AddAsync(user, default))
                .ReturnsAsync((EntityEntry<Users>)null);

            _dbContextMock.Setup(db => db.SaveEfContextChanges(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            await _repository.AddAsync(user);

            _dbContextMock.Verify(db => db.Users.AddAsync(user, default), Times.Once);
            _dbContextMock.Verify(db => db.SaveEfContextChanges(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        // 🔹 Validación de `UpdateAsync()`
        [Fact]
        public async Task UpdateUsersAsync_ShouldUpdateUserSuccessfully()
        {
            var userId = UserId.Create(Guid.NewGuid());
            var userName = UserName.Create("Updated User");
            var userEmail = UserEmail.Create("updated@example.com");

            var user = new Users
            {
                UserId = userId,
                UserName = userName,
                UserEmail = userEmail
            };

            // Simular `Update()`
            _dbContextMock.Setup(db => db.Users.Update(user));

            // Simular `SaveEfContextChanges`
            _dbContextMock.Setup(db => db.SaveEfContextChanges(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await _repository.UpdateUsersAsync(userId, user);

            // ✅ Validaciones
            Assert.NotNull(result);
            Assert.Equal("Updated User", result.UserName.Value);
            Assert.Equal("updated@example.com", result.UserEmail.Value);
            Assert.Equal(userId.Value, result.UserId.Value);

            // ✅ Verificaciones
            _dbContextMock.Verify(db => db.Users.Update(user), Times.Once);
            _dbContextMock.Verify(db => db.SaveEfContextChanges(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DeleteUsersAsync_ShouldDeleteUserSuccessfully()
        {
            var userId = UserId.Create(Guid.NewGuid());
            var existingUser = new Users { UserId = userId, UserName = UserName.Create("John Doe") };

            // Simular `FindAsync()`
            _dbContextMock.Setup(db => db.Users.FindAsync(userId))
                .ReturnsAsync(existingUser);

            // Simular `SaveEfContextChanges()`
            _dbContextMock.Setup(db => db.SaveEfContextChanges(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await _repository.DeleteUsersAsync(userId);

            //  Validaciones
            Assert.NotNull(result);
            Assert.Equal(userId.Value, result.UserId.Value);
            Assert.Equal("John Doe", result.UserName.Value);

            //  Verificaciones
            _dbContextMock.Verify(db => db.Users.FindAsync(userId), Times.Once);
            _dbContextMock.Verify(db => db.SaveEfContextChanges(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
