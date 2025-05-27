using AutoMapper;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using MongoDB.Driver;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Commoon.Dtos.Users.Response.Support;
using UserMs.Core.Database;
using UserMs.Domain.Entities.Support;
using UserMs.Domain.Entities;
using UserMs.Domain.Entities.IUser.ValueObjects;
using UserMs.Infrastructure.Repositories.Support;
using Xunit;

namespace UserMs.Test.Infrastructure.Repositories.Postgres
{
    public class SupportRepositoryTests
    {
        private Mock<IUserDbContext> _dbContextMock;
        private Mock<IMapper> _mapperMock;
        private SupportRepository _repository;

        public SupportRepositoryTests()
        {
            _dbContextMock = new Mock<IUserDbContext>();
          
            _mapperMock = new Mock<IMapper>();

         


            _repository = new SupportRepository(_dbContextMock.Object, _mapperMock.Object);
        }

        // 🔹 Validación del Constructor
        [Fact]
        public void Constructor_ShouldThrowException_WhenDbContextIsNull()
        {
           Assert.Throws<ArgumentNullException>(() => new SupportRepository(null, _mapperMock.Object));
        }


        [Fact]
        public void Constructor_ShouldThrowException_WhenMapperIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new SupportRepository(_dbContextMock.Object,  null));
        }

       
        // 🔹 Validación de `AddAsync()`
        [Fact]
        public async Task AddAsync_ShouldAddSupportSuccessfully()
        {
            var support = new Supports
            {
                UserId = UserId.Create(Guid.NewGuid()),
                UserName = UserName.Create("Test")
            };

            var mockEntityEntry = new Mock<EntityEntry<Supports>>();
            mockEntityEntry.Setup(e => e.Entity).Returns(support);

            _dbContextMock.Setup(db => db.Supports.AddAsync(support, default))
                .ReturnsAsync((EntityEntry<Supports>)null);

            _dbContextMock.Setup(db => db.SaveEfContextChanges(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            await _repository.AddAsync(support);

            _dbContextMock.Verify(db => db.Supports.AddAsync(support, default), Times.Once);
            _dbContextMock.Verify(db => db.SaveEfContextChanges(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }
        [Fact]
        public async Task UpdateAsync_ShouldUpdateSupportSuccessfully()
        {
            var supportId = UserId.Create(Guid.NewGuid());
            var supportEmail = UserEmail.Create("support@example.com");
            var supportName = UserName.Create("Test");

            var support = new Supports
            {
                UserId = supportId,
                UserName = supportName,
                UserEmail = supportEmail
            };

            // Simular `Update()`
            _dbContextMock.Setup(db => db.Supports.Update(support));

            // Simular `SaveEfContextChanges`
            _dbContextMock.Setup(db => db.SaveEfContextChanges(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await _repository.UpdateAsync(supportId, support);

            // ✅ Validaciones
            Assert.NotNull(result);
            Assert.Equal("Test", result.UserName.Value);
            Assert.Equal("support@example.com", result.UserEmail.Value);
            Assert.Equal(supportId.Value, result.UserId.Value);

            // ✅ Verificaciones
            _dbContextMock.Verify(db => db.Supports.Update(support), Times.Once);
            _dbContextMock.Verify(db => db.SaveEfContextChanges(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ShouldDeleteSupportSuccessfully()
        {
            var supportId = UserId.Create(Guid.NewGuid());
            var supportName = UserName.Create("Test");
            var existingSupport = new Supports { UserId = supportId, UserName = supportName };
           
            // Simular `FindAsync`
            _dbContextMock.Setup(db => db.Supports.FindAsync(supportId))
                .ReturnsAsync(existingSupport);

            // Simular `SaveEfContextChanges`
            _dbContextMock.Setup(db => db.SaveEfContextChanges(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await _repository.DeleteAsync(supportId);

            // ✅ Validaciones
            Assert.NotNull(result);
            Assert.Equal(supportId.Value, result.UserId.Value);
            Assert.Equal("Test", result.UserName.Value);

            // ✅ Verificaciones
            _dbContextMock.Verify(db => db.Supports.FindAsync(supportId), Times.Once);
            _dbContextMock.Verify(db => db.SaveEfContextChanges(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }
       
    }
}
