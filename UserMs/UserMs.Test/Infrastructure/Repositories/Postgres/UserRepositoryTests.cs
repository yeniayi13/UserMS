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

namespace UserMs.Test.Infrastructure.Repositories.Postgres
{
    public class UsersRepositoryTests
    {
        private Mock<IUserDbContext> _dbContextMock;
        private Mock<IMapper> _mapperMock;
        private UsersRepository _repository;

        public UsersRepositoryTests()
        {
            _dbContextMock = new Mock<IUserDbContext>();
            _mapperMock = new Mock<IMapper>();

            var mockDatabase = new Mock<IMongoDatabase>();
            

            _repository = new UsersRepository(_dbContextMock.Object,  _mapperMock.Object);
        }

        // 🔹 Validación del Constructor
        [Fact]
        public void Constructor_ShouldThrowException_WhenDbContextIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new UsersRepository(null,  _mapperMock.Object));
        }


        [Fact]
        public void Constructor_ShouldThrowException_WhenMapperIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new UsersRepository(_dbContextMock.Object, null));
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
