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
using Xunit;
using UserMs.Infrastructure.Repositories;
using UserMs.Infrastructure.Repositories.Users;

namespace UserMs.Test.Infrastructure.Repositories.Mongo
{
    public class UserRepositoryMongoTests
    {
       
        private Mock<IUserDbContextMongo> _mongoContextMock;
        private Mock<IMongoCollection<Users>> _mongoCollectionMock;
        private Mock<IMapper> _mapperMock;
        private UsersRepositoryMongo _repository;
        public UserRepositoryMongoTests()
        {
            
            _mongoContextMock = new Mock<IUserDbContextMongo>();
            _mongoCollectionMock = new Mock<IMongoCollection<Users>>();
            _mapperMock = new Mock<IMapper>();

            var mockDatabase = new Mock<IMongoDatabase>();
            mockDatabase.Setup(db => db.GetCollection<Users>("Users", null))
                .Returns(_mongoCollectionMock.Object);

            _mongoContextMock.Setup(m => m.Database).Returns(mockDatabase.Object);

              _repository = new UsersRepositoryMongo( _mongoContextMock.Object, _mapperMock.Object);
        }

        // 🔹 Validación del Constructor
        

        [Fact]
        public void Constructor_ShouldThrowException_WhenMongoContextIsNull()
        {
              Assert.Throws<ArgumentNullException>(() => new UsersRepository( null, _mapperMock.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowException_WhenMapperIsNull()
        {
              Assert.Throws<ArgumentNullException>(() => new UsersRepositoryMongo(_mongoContextMock.Object, null));
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

              var result = await _repository.GetUsersAsync();

              Assert.NotNull(result);
              Assert.Empty(result);
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

             var result = await _repository.GetUsersAsync();

             Assert.NotNull(result);
             Assert.Equal(2, result.Count);
             Assert.Equal(usersDtos[0].UserId, result[0].UserId.Value);
             Assert.Equal("Maria", result[0].UserName.Value);
             Assert.Equal(usersDtos[1].UserId, result[1].UserId.Value);
             Assert.Equal("Jose", result[1].UserName.Value);
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

             var result = await _repository.GetUsersById(userId);

             Assert.NotNull(result);
             Assert.Equal(userId, result.UserId.Value);
             Assert.Equal("Jose", result.UserName.Value);
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

             var result = await _repository.GetUsersById(userId);

             Assert.Null(result); //  Validación final
        }

        [Fact]
        public async Task GetUsersByEmail_ShouldReturnUser_WhenUserExists()
        {
            var userEmail = "user@example.com";
            var userDto = new GetUsersDto { UserId = Guid.NewGuid(), UserEmail = userEmail, UserName = "Test User" };

            var mockCursor = new Mock<IAsyncCursor<GetUsersDto>>();
            mockCursor.SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)
                .ReturnsAsync(false);
            mockCursor.SetupGet(c => c.Current).Returns(new List<GetUsersDto> { userDto });

            _mongoCollectionMock.Setup(c => c.FindAsync(
                It.IsAny<FilterDefinition<Domain.Entities.UserEntity.Users>>(),
                It.IsAny<FindOptions<Domain.Entities.UserEntity.Users, GetUsersDto>>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockCursor.Object);

            _mapperMock.Setup(m => m.Map<Domain.Entities.UserEntity.Users>(userDto))
                .Returns(new Domain.Entities.UserEntity.Users
                {
                    UserId = UserId.Create(userDto.UserId),
                    UserEmail = UserEmail.Create(userDto.UserEmail),
                    UserName = UserName.Create(userDto.UserName)
                });

            var result = await _repository.GetUsersByEmail(userEmail);

            Assert.NotNull(result);
            Assert.Equal(userEmail, result.UserEmail.Value);
            Assert.Equal("Test User", result.UserName.Value);
        }

        // ✅ Prueba: `GetUsersByEmail()` cuando el usuario no existe
        [Fact]
        public async Task GetUsersByEmail_ShouldReturnNull_WhenUserNotFound()
        {
            var userEmail = "user@example.com";

            var mockCursor = new Mock<IAsyncCursor<GetUsersDto>>();
            mockCursor.SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);
            mockCursor.SetupGet(c => c.Current).Returns(new List<GetUsersDto>());

            _mongoCollectionMock.Setup(c => c.FindAsync(
                It.IsAny<FilterDefinition<Domain.Entities.UserEntity.Users>>(),
                It.IsAny<FindOptions<Domain.Entities.UserEntity.Users, GetUsersDto>>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockCursor.Object);

            var result = await _repository.GetUsersByEmail(userEmail);

            Assert.Null(result);
        }



    }
}
