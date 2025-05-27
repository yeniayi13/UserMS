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
using UserMs.Domain.Entities.IUser.ValueObjects;
using UserMs.Domain.Entities;
using UserMs.Infrastructure.Repositories.Bidder;
using UserMs.Infrastructure.Repositories.Bidders;
using Xunit;

namespace UserMs.Test.Infrastructure.Repositories.Mongo
{
    public class BidderRepositoryMongoTests
    {
        private Mock<IUserDbContextMongo> _mongoContextMock;
        private Mock<IMongoCollection<Bidders>> _mongoCollectionMock;
        private Mock<IMapper> _mapperMock;
        private BidderRepositoryMongo _repository;

        public BidderRepositoryMongoTests()
        {
           
            _mongoContextMock = new Mock<IUserDbContextMongo>();
            _mongoCollectionMock = new Mock<IMongoCollection<Bidders>>();
            _mapperMock = new Mock<IMapper>();

            var mockDatabase = new Mock<IMongoDatabase>();
            mockDatabase.Setup(db => db.GetCollection<Bidders>("Bidders", null))
                .Returns(_mongoCollectionMock.Object);

            _mongoContextMock.Setup(m => m.Database).Returns(mockDatabase.Object);

             _repository = new BidderRepositoryMongo( _mongoContextMock.Object, _mapperMock.Object);
        }

        // 🔹 Validación del Constructor
       

        [Fact]
        public void Constructor_ShouldThrowException_WhenMongoContextIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new BidderRepositoryMongo( null, _mapperMock.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowException_WhenMongoDatabaseIsNull()
        {
            _mongoContextMock.Setup(m => m.Database).Returns((IMongoDatabase)null);
             Assert.Throws<ArgumentNullException>(() => new BidderRepositoryMongo(_mongoContextMock.Object, _mapperMock.Object));
        }



        [Fact]
        public void Constructor_ShouldThrowException_WhenMapperIsNull()
        {
             Assert.Throws<ArgumentNullException>(() => new BidderRepositoryMongo(_mongoContextMock.Object, null));
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

              var result = await _repository.GetBidderAllAsync();

              Assert.NotNull(result);
              Assert.Empty(result);
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

             var result = await _repository.GetBidderByEmailAsync(bidderEmail);

             Assert.NotNull(result);
             Assert.Equal(bidderDto.UserId, result.UserId.Value);
             Assert.Equal(bidderName.Value, result.UserName.Value);
             Assert.Equal(bidderEmail, result.UserEmail);
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

             var result = await _repository.GetBidderByIdAsync(bidderId);

             Assert.Null(result); // ✅ Validación final*/
        }

        [Fact]
        public async Task GetBidderByIdAsync_ShouldReturnBidder_WhenFound()
        {
            var bidderId = UserId.Create(Guid.NewGuid());
            var bidderName = UserName.Create("Name");
            var bidderDto = new GetBidderDto { UserId = Guid.NewGuid(), UserName = "Name" };
            var bidderEntity = new Bidders { UserId = bidderDto.UserId, UserName = bidderName };

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

             var result = await _repository.GetBidderByIdAsync(bidderId);

             Assert.NotNull(result);
             Assert.Equal(bidderDto.UserId, result.UserId.Value);
             Assert.Equal("Name", result.UserName.Value);
        }

       
        

       

    }
}
