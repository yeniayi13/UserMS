using AutoMapper;
using MongoDB.Driver;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Commoon.Dtos.Users.Response.Auctioneer;
using UserMs.Core.Database;
using UserMs.Domain.Entities.Auctioneer;
using UserMs.Domain.Entities;
using UserMs.Domain.Entities.Auctioneer.ValueObjects;
using UserMs.Domain.Entities.IUser.ValueObjects;
using UserMs.Infrastructure.Repositories.Auctioneer;
using Xunit;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace UserMs.Test.Infrastructure.Repositories.Postgres
{
    public class AuctioneerRepositoryTests
    {
        private Mock<IUserDbContext> _dbContextMock;
        private Mock<IMapper> _mapperMock;
        private AuctioneerRepository _repository;

        public AuctioneerRepositoryTests()
        {
            _dbContextMock = new Mock<IUserDbContext>();
           
            _mapperMock = new Mock<IMapper>();


            _repository = new AuctioneerRepository(_dbContextMock.Object, _mapperMock.Object);
        }

        // 🔹 Validación del Constructor
        [Fact]
        public void Constructor_ShouldThrowException_WhenDbContextIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new AuctioneerRepository(null, _mapperMock.Object));
        }


        [Fact]
        public void Constructor_ShouldThrowException_WhenMapperIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new AuctioneerRepository(_dbContextMock.Object, null));
        }

        [Fact]
        public async Task AddAsync_ShouldAddAuctioneerSuccessfully()
        {
            var auctioneer = new Auctioneers
            {
                UserId = UserId.Create(Guid.NewGuid()),
                UserName = UserName.Create("Jose")
            };

            // Simular `EntityEntry<Auctioneers>`
            var mockEntityEntry = new Mock<EntityEntry<Auctioneers>>();
            mockEntityEntry.Setup(e => e.Entity).Returns(auctioneer);

            // Simular `AddAsync`
            _dbContextMock.Setup(db => db.Auctioneers.AddAsync(auctioneer, default))
                .ReturnsAsync((EntityEntry<Auctioneers>)null); // EF no devuelve resultado en pruebas

            // Simular `SaveEfContextChanges`
            _dbContextMock.Setup(db => db.SaveEfContextChanges(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            await _repository.AddAsync(auctioneer);

            // Verificar `AddAsync`
            _dbContextMock.Verify(db => db.Auctioneers.AddAsync(auctioneer, default), Times.Once);

            // Verificar `SaveEfContextChanges`
            _dbContextMock.Verify(db => db.SaveEfContextChanges(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateAuctioneerSuccessfully()
        {
            var auctioneerId = UserId.Create(Guid.NewGuid());
            var auctionerName = UserName.Create("Updated Name");
            var auctioneer = new Auctioneers { UserId = auctioneerId, UserName = auctionerName };

            // Simular `Update()`
            _dbContextMock.Setup(db => db.Auctioneers.Update(auctioneer));

            // Simular `SaveEfContextChanges`
            _dbContextMock.Setup(db => db.SaveEfContextChanges(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await _repository.UpdateAsync(auctioneerId, auctioneer);

            // Validar que la entidad fue actualizada correctamente
            Assert.NotNull(result);
            Assert.Equal("Updated Name", result.UserName.Value);

            // Verificar `Update()`
            _dbContextMock.Verify(db => db.Auctioneers.Update(auctioneer), Times.Once);

            // Verificar `SaveEfContextChanges`
            _dbContextMock.Verify(db => db.SaveEfContextChanges(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ShouldDeleteAuctioneerSuccessfully()
        {
            var auctioneerId = UserId.Create(Guid.NewGuid());
            var auctionerName = UserName.Create("Jose");
            var existingAuctioneer = new Auctioneers { UserId = auctioneerId, UserName = auctionerName };

            // Simular `FindAsync`
            _dbContextMock.Setup(db => db.Auctioneers.FindAsync(auctioneerId))
                .ReturnsAsync(existingAuctioneer);

            // Simular `SaveEfContextChanges`
            _dbContextMock.Setup(db => db.SaveEfContextChanges(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await _repository.DeleteAsync(auctioneerId);

            // Validar que el resultado es el esperado
            Assert.NotNull(result);
            Assert.Equal(auctioneerId.Value, result.UserId.Value);

            // Verificar `FindAsync`
            _dbContextMock.Verify(db => db.Auctioneers.FindAsync(auctioneerId), Times.Once);

            // Verificar `SaveEfContextChanges`
            _dbContextMock.Verify(db => db.SaveEfContextChanges(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        
    }
}
