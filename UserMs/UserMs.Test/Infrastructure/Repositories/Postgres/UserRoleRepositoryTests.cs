using AutoMapper;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Core.Database;
using UserMs.Domain.Entities.Role.ValueObjects;
using UserMs.Domain.Entities;
using UserMs.Domain.User_Roles;
using UserMs.Domain.User_Roles.ValueObjects;
using UserMs.Infrastructure.Repositories.User_Roles;
using Xunit;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using UserMs.Domain.Entities.Support;

namespace UserMs.Test.Infrastructure.Repositories.Postgres
{
    public class UserRolesRepositoryTests
    {
        private readonly Mock<IUserDbContext> _dbContextMock = new();
        private readonly Mock<IMapper> _mapperMock = new();
        private readonly UserRolesRepository _repository;

        public UserRolesRepositoryTests()
        {
            _repository = new UserRolesRepository(_dbContextMock.Object, _mapperMock.Object);
        }

        // ✅ Validación del Constructor
        [Fact]
        public void Constructor_ShouldThrowException_WhenDbContextIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new UserRolesRepository(null, _mapperMock.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowException_WhenMapperIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new UserRolesRepository(_dbContextMock.Object, null));
        }

        // ✅ Validación de `AddAsync()`
        [Fact]
        public async Task AddAsync_ShouldAddUserRoleSuccessfully()
        {
            var userRole = new UserRoles(UserRoleId.Create(Guid.NewGuid()), UserId.Create(Guid.NewGuid()), RoleId.Create(Guid.NewGuid()));

            _dbContextMock.Setup(db => db.UserRoles.AddAsync(userRole, default))
                .ReturnsAsync((EntityEntry<UserRoles>)null);

            _dbContextMock.Setup(db => db.SaveEfContextChanges(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            await _repository.AddAsync(userRole);

            _dbContextMock.Verify(db => db.UserRoles.AddAsync(userRole,default), Times.Once);
            _dbContextMock.Verify(db => db.SaveEfContextChanges(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        // ✅ Validación de `DeleteUsersRoleAsync()`
        [Fact]
        public async Task DeleteUsersRoleAsync_ShouldDeleteUserRoleSuccessfully_WhenExists()
        {
            var userRoleId = UserRoleId.Create(Guid.NewGuid());
            var existingUserRole = new UserRoles(userRoleId, UserId.Create(Guid.NewGuid()), RoleId.Create(Guid.NewGuid()));

            _dbContextMock.Setup(db => db.UserRoles.FindAsync(userRoleId))
                .ReturnsAsync(existingUserRole);

            _dbContextMock.Setup(db => db.SaveEfContextChanges(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await _repository.DeleteUsersRoleAsync(userRoleId);

            Assert.NotNull(result);
            _dbContextMock.Verify(db => db.UserRoles.Remove(existingUserRole), Times.Once);
            _dbContextMock.Verify(db => db.SaveEfContextChanges(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        // ✅ Validación de `DeleteUsersRoleAsync()` cuando el usuario no existe
        [Fact]
        public async Task DeleteUsersRoleAsync_ShouldReturnNull_WhenUserRoleDoesNotExist()
        {
            var userRoleId = UserRoleId.Create(Guid.NewGuid());

            _dbContextMock.Setup(db => db.UserRoles.FindAsync(userRoleId))
                .ReturnsAsync((UserRoles)null);

            var result = await _repository.DeleteUsersRoleAsync(userRoleId);

            Assert.Null(result);
            _dbContextMock.Verify(db => db.UserRoles.Remove(It.IsAny<UserRoles>()), Times.Never);
            _dbContextMock.Verify(db => db.SaveEfContextChanges(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        // ✅ Validación de `UpdateUsersRoleAsync()`
        [Fact]
        public async Task UpdateUsersRoleAsync_ShouldUpdateUserRoleSuccessfully()
        {
            var userRole = new UserRoles(UserRoleId.Create(Guid.NewGuid()), UserId.Create(Guid.NewGuid()), RoleId.Create(Guid.NewGuid()));

            _dbContextMock.Setup(db => db.UserRoles.Update(It.IsAny<UserRoles>()));
            _dbContextMock.Setup(db => db.SaveEfContextChanges(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await _repository.UpdateUsersRoleAsync(userRole.UserId, userRole);

            Assert.NotNull(result);
            _dbContextMock.Verify(db => db.UserRoles.Update(userRole), Times.Once);
            _dbContextMock.Verify(db => db.SaveEfContextChanges(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }

}
