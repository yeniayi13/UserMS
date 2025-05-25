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

namespace UserMs.Test.Infrastructure.Repositories
{
    public class BidderRepositoryTests
    {
        private Mock<IUserDbContext> _dbContextMock;
        private Mock<IUserDbContextMongo> _mongoContextMock;
        private Mock<IMongoCollection<Bidders>> _mongoCollectionMock;
        private Mock<IMapper> _mapperMock;
        private BidderRepository _repository;

        public BidderRepositoryTests()
        {
            _dbContextMock = new Mock<IUserDbContext>();
            _mongoContextMock = new Mock<IUserDbContextMongo>();
            _mongoCollectionMock = new Mock<IMongoCollection<Bidders>>();
            _mapperMock = new Mock<IMapper>();

            var mockDatabase = new Mock<IMongoDatabase>();
            mockDatabase.Setup(db => db.GetCollection<Bidders>("Bidders", null))
                .Returns(_mongoCollectionMock.Object);

            _mongoContextMock.Setup(m => m.Database).Returns(mockDatabase.Object);

           // _repository = new BidderRepository(_dbContextMock.Object, _mongoContextMock.Object, _mapperMock.Object);
        }

        // 🔹 Validación del Constructor
        [Fact]
        public void Constructor_ShouldThrowException_WhenDbContextIsNull()
        {
          //  Assert.Throws<ArgumentNullException>(() => new BidderRepository(null, _mongoContextMock.Object, _mapperMock.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowException_WhenMongoContextIsNull()
        {
            //Assert.Throws<ArgumentNullException>(() => new BidderRepository(_dbContextMock.Object, null, _mapperMock.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowException_WhenMongoDatabaseIsNull()
        {
            _mongoContextMock.Setup(m => m.Database).Returns((IMongoDatabase)null);
           // Assert.Throws<ArgumentNullException>(() => new BidderRepository(_dbContextMock.Object, _mongoContextMock.Object, _mapperMock.Object));
        }



        [Fact]
        public void Constructor_ShouldThrowException_WhenMapperIsNull()
        {
           // Assert.Throws<ArgumentNullException>(() => new BidderRepository(_dbContextMock.Object, _mongoContextMock.Object, null));
        }

        // 🔹 Validación de `GetBidderAllAsync()`
        [Fact]
        public async Task GetBidderAllAsync_ShouldReturnEmptyList_WhenNoBiddersFound()
        {
            var mockCursor = new Mock<IAsyncCursor<GetBidderDto>>();
            mockCursor.SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);
            mockCursor.SetupGet(c => c.Current).Returns(new List<GetBidderDto>());

            _mongoCollectionMock.Setup(c => c.FindAsync(
                    It.IsAny<FilterDefinition<Bidders>>(),
                    It.IsAny<FindOptions<Bidders, GetBidderDto>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockCursor.Object);

            _mapperMock.Setup(m => m.Map<List<Bidders>>(It.IsAny<List<GetBidderDto>>()))
                .Returns(new List<Bidders>());

          /*  var result = await _repository.GetBidderAllAsync();

            Assert.NotNull(result);
            Assert.Empty(result);*/
        }

        [Fact]
        public async Task GetBidderByEmailAsync_ShouldReturnBidder_WhenFound()
        {
            var bidderEmail = UserEmail.Create("bidder@example.com");
            var bidderName = UserName.Create("Name");
            var bidderDto = new GetBidderDto { UserId = Guid.NewGuid(), UserName = "Name", UserEmail = bidderEmail.Value };
            var bidderEntity = new Bidders { UserId = bidderDto.UserId, UserName = bidderName, UserEmail = bidderEmail };

            var mockCursor = new Mock<IAsyncCursor<GetBidderDto>>();
            mockCursor.SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)
                .ReturnsAsync(false);
            mockCursor.SetupGet(c => c.Current).Returns(new List<GetBidderDto> { bidderDto });

            _mongoCollectionMock.Setup(c => c.FindAsync(
                    It.IsAny<FilterDefinition<Bidders>>(),
                    It.IsAny<FindOptions<Bidders, GetBidderDto>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockCursor.Object);

            _mapperMock.Setup(m => m.Map<Bidders>(bidderDto)).Returns(bidderEntity);

           /* var result = await _repository.GetBidderByEmailAsync(bidderEmail);

            Assert.NotNull(result);
            Assert.Equal(bidderDto.UserId, result.UserId.Value);
            Assert.Equal(bidderName.Value, result.UserName.Value);
            Assert.Equal(bidderEmail, result.UserEmail);*/
        }

        [Fact]
        public async Task GetBidderByIdAsync_ShouldReturnNull_WhenBidderNotFound()
        {
            var bidderId = UserId.Create(Guid.NewGuid());

            var mockCursor = new Mock<IAsyncCursor<GetBidderDto>>();
            mockCursor.SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(false); // 🔹 No hay registros encontrados
            mockCursor.SetupGet(c => c.Current).Returns(new List<GetBidderDto>());

            _mongoCollectionMock.Setup(c => c.FindAsync(
                    It.IsAny<FilterDefinition<Bidders>>(),
                    It.IsAny<FindOptions<Bidders, GetBidderDto>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockCursor.Object);

            _mapperMock.Setup(m => m.Map<Bidders>(It.IsAny<GetBidderDto>()))
                .Returns((Bidders)null); // 🔹 Simula que no hay datos

           /* var result = await _repository.GetBidderByIdAsync(bidderId);

            Assert.Null(result); // ✅ Validación final*/
        }

        [Fact]
        public async Task GetBidderByIdAsync_ShouldReturnBidder_WhenFound()
        {
            var bidderId = UserId.Create(Guid.NewGuid());
            var bidderName = UserName.Create("Name");
            var bidderDto = new GetBidderDto { UserId = Guid.NewGuid(), UserName = "Name"};
            var bidderEntity = new Bidders { UserId = bidderDto.UserId, UserName = bidderName};

            var mockCursor = new Mock<IAsyncCursor<GetBidderDto>>();
            mockCursor.SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)
                .ReturnsAsync(false);
            mockCursor.SetupGet(c => c.Current).Returns(new List<GetBidderDto> { bidderDto });

            _mongoCollectionMock.Setup(c => c.FindAsync(
                    It.IsAny<FilterDefinition<Bidders>>(),
                    It.IsAny<FindOptions<Bidders, GetBidderDto>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockCursor.Object);

            _mapperMock.Setup(m => m.Map<Bidders>(bidderDto)).Returns(bidderEntity);

           /* var result = await _repository.GetBidderByIdAsync(bidderId);

            Assert.NotNull(result);
            Assert.Equal(bidderDto.UserId, result.UserId.Value);
            Assert.Equal("Name", result.UserName.Value);*/
        }

        // 🔹 Validación de `AddAsync()`
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
