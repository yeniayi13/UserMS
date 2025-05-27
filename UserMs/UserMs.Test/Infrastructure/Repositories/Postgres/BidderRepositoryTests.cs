using AutoMapper;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using MongoDB.Driver;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Commoon.Dtos.Users.Response.Bidder;
using UserMs.Core.Database;
using UserMs.Domain.Entities.Bidder;
using UserMs.Domain.Entities;
using UserMs.Infrastructure.Repositories.Bidder;
using Xunit;
using UserMs.Domain.Entities.IUser.ValueObjects;

namespace UserMs.Test.Infrastructure.Repositories.Postgres
{
    public class BidderRepositoryTests
    {
        private Mock<IUserDbContext> _dbContextMock;
        private Mock<IMapper> _mapperMock;
        private BidderRepository _repository;

        public BidderRepositoryTests()
        {
            _dbContextMock = new Mock<IUserDbContext>();

            _mapperMock = new Mock<IMapper>();


            _repository = new BidderRepository(_dbContextMock.Object, _mapperMock.Object);
        }

        // 🔹 Validación del Constructor
        [Fact]
        public void Constructor_ShouldThrowException_WhenDbContextIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new BidderRepository(null, _mapperMock.Object));
        }

       



        [Fact]
        public void Constructor_ShouldThrowException_WhenMapperIsNull()
        { Assert.Throws<ArgumentNullException>(() => new BidderRepository(_dbContextMock.Object,  null));
        }

       
      
        [Fact]
        public async Task AddAsync_ShouldAddBidderSuccessfully()
        {
            var bidder = new Bidders
            {
                UserId = UserId.Create(Guid.NewGuid()),
                UserName = UserName.Create("Name")
            };

            var mockEntityEntry = new Mock<EntityEntry<Bidders>>();
            mockEntityEntry.Setup(e => e.Entity).Returns(bidder);

            _dbContextMock.Setup(db => db.Bidders.AddAsync(bidder, default))
                .ReturnsAsync((EntityEntry<Bidders>)null);

            _dbContextMock.Setup(db => db.SaveEfContextChanges(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            await _repository.AddAsync(bidder);

            _dbContextMock.Verify(db => db.Bidders.AddAsync(bidder, default), Times.Once);
            _dbContextMock.Verify(db => db.SaveEfContextChanges(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateBidderSuccessfully()
        {
            var bidderName = UserName.Create("Updated Name");
            var bidderId = UserId.Create(Guid.NewGuid());
            var bidder = new Bidders { UserId = bidderId, UserName = bidderName };

            _dbContextMock.Setup(db => db.Bidders.Update(bidder));

            _dbContextMock.Setup(db => db.SaveEfContextChanges(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await _repository.UpdateAsync(bidderId, bidder);

            Assert.NotNull(result);
            Assert.Equal("Updated Name", result.UserName.Value);

            _dbContextMock.Verify(db => db.Bidders.Update(bidder), Times.Once);
            _dbContextMock.Verify(db => db.SaveEfContextChanges(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ShouldDeleteBidderSuccessfully()
        {
            var bidderName = UserName.Create("Updated Name");
            var bidderId = UserId.Create(Guid.NewGuid());
            var existingBidder = new Bidders { UserId = bidderId, UserName = bidderName };
            

            _dbContextMock.Setup(db => db.Bidders.FindAsync(bidderId))
                .ReturnsAsync(existingBidder);

            _dbContextMock.Setup(db => db.SaveEfContextChanges(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await _repository.DeleteAsync(bidderId);

            Assert.NotNull(result);
            Assert.Equal(bidderId, result.UserId);

            _dbContextMock.Verify(db => db.Bidders.FindAsync(bidderId), Times.Once);
            _dbContextMock.Verify(db => db.SaveEfContextChanges(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
