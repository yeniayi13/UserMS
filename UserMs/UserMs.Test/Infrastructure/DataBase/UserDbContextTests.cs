using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Domain.Entities;
using UserMs.Domain.Entities.IUser.ValueObjects;
using UserMs.Domain.Entities.UserEntity;
using UserMs.Infrastructure.Database.Context.Postgress;
using Xunit;

namespace UserMs.Test.Infrastructure.DataBase
{
    public class UserDbContextTests
    {
        private readonly DbContextOptions<UserDbContext> _options;
        private UserDbContext _dbContext;

        public UserDbContextTests()
        {
            _options = new DbContextOptionsBuilder<UserDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            _dbContext = new UserDbContext(_options);
        }




        [Fact]
        public void BeginTransaction_ShouldThrowNotSupportedException_WhenUsingInMemoryDatabase()
        {
            // 🔹 Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => _dbContext.BeginTransaction());

            Assert.Contains("Transactions are not supported by the in-memory store", exception.Message);
        }

        [Fact]
        public void Constructor_ShouldInitializeDbContext()
        {
            Assert.NotNull(_dbContext);
            Assert.NotNull(_dbContext.DbContext);
        }

        [Fact]
        public void ChangeEntityState_ShouldChangeState_WhenEntityIsNotNull()
        {
            var user = new Users(UserId.Create(Guid.NewGuid()), UserName.Create("TestUser"));
            _dbContext.ChangeEntityState(user, EntityState.Modified);
            Assert.Equal(EntityState.Modified, _dbContext.Entry(user).State);
        }

        [Fact]
        public void SetPropertyIsModifiedToFalse_ShouldMarkPropertyAsUnmodified()
        {
            var user = new Users(UserId.Create(Guid.NewGuid()), UserName.Create("TestUser"));
            _dbContext.Entry(user).Property(u => u.UserName).IsModified = true;
            _dbContext.SetPropertyIsModifiedToFalse(user, u => u.UserName);
            Assert.False(_dbContext.Entry(user).Property(u => u.UserName).IsModified);
        }

        [Fact]
        public async Task SaveChangesAsync_ShouldUpdateTimestamps_OnAddedEntity()
        {
            var user = new Users(
                UserId.Create(Guid.NewGuid()),
                UserEmail.Create("testuser@example.com"),
                UserPassword.Create("SecurePassword123"),
                UserName.Create("TestUser"),
                UserPhone.Create("0212657963"),
                UserAddress.Create("123 Example St"),
                UserLastName.Create("Doe"),
                UsersType.Subastador,
                UserAvailable.Activo

            );
            user.CreatedAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;
            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();
            Assert.NotNull(user.CreatedAt);
            Assert.NotNull(user.UpdatedAt);
            Assert.InRange(DateTime.UtcNow, DateTime.UtcNow.AddMilliseconds(-1), DateTime.UtcNow.AddMilliseconds(1));
        }

        [Fact]
        public async Task SaveChangesAsync_WithUser_ShouldUpdateCreatedByAndUpdatedBy()
        {
            var user = new Users(
                UserId.Create(Guid.NewGuid()),
                UserEmail.Create("testuser@example.com"),
                UserPassword.Create("SecurePassword123"),
                UserName.Create("TestUser"),
                UserPhone.Create("0212657963"),
                UserAddress.Create("123 Example St"),
                UserLastName.Create("Doe"),
                UsersType.Subastador,
                UserAvailable.Activo

            );
            user.CreatedBy = "TestUser";
            user.UpdatedBy = "TestUser";
            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync("TestUser");
            Assert.Equal("TestUser", user.CreatedBy);
            Assert.Equal("TestUser", user.UpdatedBy);
        }

        [Fact]
        public async Task SaveEfContextChanges_ShouldReturnTrue_WhenChangesAreSaved()
        {
            var user = new Users(
                UserId.Create(Guid.NewGuid()),
                UserEmail.Create("testuser@example.com"),
                UserPassword.Create("SecurePassword123"),
                UserName.Create("TestUser"),
                UserPhone.Create("0212657963"),
                UserAddress.Create("123 Example St"),
                UserLastName.Create("Doe"),
                UsersType.Subastador,
                UserAvailable.Activo

            );
            _dbContext.Users.Add(user);
            var result = await _dbContext.SaveEfContextChanges("TestUser");
            Assert.True(result);
        }

        [Fact]
        public async Task SaveEfContextChanges_ShouldReturnFalse_WhenNoChangesAreSaved()
        {
            var result = await _dbContext.SaveEfContextChanges("User");
            Assert.False(result);
        }
    }
}
