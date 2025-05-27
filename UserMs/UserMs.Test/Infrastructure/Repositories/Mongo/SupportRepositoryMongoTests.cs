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
using UserMs.Domain.Entities.IUser.ValueObjects;
using UserMs.Domain.Entities.Support;
using UserMs.Domain.Entities;
using UserMs.Infrastructure.Repositories.Support;
using UserMs.Infrastructure.Repositories.Supports;
using Xunit;

namespace UserMs.Test.Infrastructure.Repositories.Mongo
{
    public class SupportRepositoryMongoTests
    {
     
        private Mock<IUserDbContextMongo> _mongoContextMock;
        private Mock<IMongoCollection<Supports>> _mongoCollectionMock;
        private Mock<IMapper> _mapperMock;
        private SupportRepositoryMongo _repository;

        public SupportRepositoryMongoTests()
        {
           
            _mongoContextMock = new Mock<IUserDbContextMongo>();
            _mongoCollectionMock = new Mock<IMongoCollection<Supports>>();
            _mapperMock = new Mock<IMapper>();

            var mockDatabase = new Mock<IMongoDatabase>();
            mockDatabase.Setup(db => db.GetCollection<Supports>("Supports", null))
                .Returns(_mongoCollectionMock.Object);

            _mongoContextMock.Setup(m => m.Database).Returns(mockDatabase.Object);

            _repository = new SupportRepositoryMongo(_mongoContextMock.Object, _mapperMock.Object);
        }

        // 🔹 Validación del Constructor


        [Fact]
        public void Constructor_ShouldThrowException_WhenMongoContextIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new SupportRepository( null, _mapperMock.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowException_WhenMongoDatabaseIsNull()
        {
            _mongoContextMock.Setup(m => m.Database).Returns((IMongoDatabase)null);
             Assert.Throws<ArgumentNullException>(() => new SupportRepositoryMongo( _mongoContextMock.Object, _mapperMock.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowException_WhenMapperIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new SupportRepositoryMongo( _mongoContextMock.Object, null));
        }

        // 🔹 Validación de `GetSupportAllAsync()`
        [Fact]
        public async Task GetSupportAllAsync_ShouldReturnEmptyList_WhenNoSupportsFound()
        {
            var mockCursor = new Mock<IAsyncCursor<GetSupportDto>>();
            mockCursor.SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);
            mockCursor.SetupGet(c => c.Current).Returns(new List<GetSupportDto>());

            _mongoCollectionMock.Setup(c => c.FindAsync(
                    It.IsAny<FilterDefinition<Supports>>(),
                    It.IsAny<FindOptions<Supports, GetSupportDto>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockCursor.Object);

            _mapperMock.Setup(m => m.Map<List<Supports>>(It.IsAny<List<GetSupportDto>>()))
                .Returns(new List<Supports>());

             var result = await _repository.GetSupportAllAsync();

             Assert.NotNull(result);
             Assert.Empty(result);
        }

        [Fact]
        public async Task GetSupportByEmailAsync_ShouldReturnSupport_WhenFound()
        {
            var supportEmail = UserEmail.Create("support@example.com");
            var supportName = UserName.Create("Test");
            var supportDto = new GetSupportDto { UserId = Guid.NewGuid(), UserName = "Test", UserEmail = supportEmail.Value };
            var supportEntity = new Supports { UserId = supportDto.UserId, UserName = supportName, UserEmail = supportEmail };

            var mockCursor = new Mock<IAsyncCursor<GetSupportDto>>();
            mockCursor.SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)
                .ReturnsAsync(false);
            mockCursor.SetupGet(c => c.Current).Returns(new List<GetSupportDto> { supportDto });

            _mongoCollectionMock.Setup(c => c.FindAsync(
                    It.IsAny<FilterDefinition<Supports>>(),
                    It.IsAny<FindOptions<Supports, GetSupportDto>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockCursor.Object);

            _mapperMock.Setup(m => m.Map<Supports>(supportDto)).Returns(supportEntity);

            var result = await _repository.GetSupportByEmailAsync(supportEmail);

            Assert.NotNull(result);
            Assert.Equal(supportDto.UserId, result.UserId.Value);
            Assert.Equal("Test", result.UserName.Value);
            Assert.Equal(supportEmail, result.UserEmail);
        }
        [Fact]
        public async Task GetSupportByNameAsync_ShouldReturnNull_WhenSupportNotFound()
        {
            var supportEmail = UserEmail.Create("support@example.com");

            var mockCursor = new Mock<IAsyncCursor<GetSupportDto>>();
            mockCursor.SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(false); // 🔹 No hay registros encontrados
            mockCursor.SetupGet(c => c.Current).Returns(new List<GetSupportDto>());

            _mongoCollectionMock.Setup(c => c.FindAsync(
                    It.IsAny<FilterDefinition<Supports>>(),
                    It.IsAny<FindOptions<Supports, GetSupportDto>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockCursor.Object);

            _mapperMock.Setup(m => m.Map<Supports>(It.IsAny<GetSupportDto>()))
                .Returns((Supports)null); // 🔹 Simula que no hay datos

            var result = await _repository.GetSupportByEmailAsync(supportEmail);

            Assert.Null(result); // ✅ Validación final
        }
      
        
        [Fact]
        public async Task GetSupportByIdAsync_ShouldReturnNull_WhenSupportNotFound()
        {
            var supportId = UserId.Create(Guid.NewGuid());

            var mockCursor = new Mock<IAsyncCursor<GetSupportDto>>();
            mockCursor.SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(false); // 🔹 No hay registros encontrados
            mockCursor.SetupGet(c => c.Current).Returns(new List<GetSupportDto>());

            _mongoCollectionMock.Setup(c => c.FindAsync(
                    It.IsAny<FilterDefinition<Supports>>(),
                    It.IsAny<FindOptions<Supports, GetSupportDto>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockCursor.Object);

            _mapperMock.Setup(m => m.Map<Supports>(It.IsAny<GetSupportDto>()))
                .Returns((Supports)null); // 🔹 Simula que no hay datos

             var result = await _repository.GetSupportByIdAsync(supportId);

            Assert.Null(result); // ✅ Validación final
        }

        [Fact]
        public async Task GetSupportByIdAsync_ShouldReturnSupport_WhenFound()
        {
            var supportId = UserId.Create(Guid.NewGuid());
            var supportEmail = UserEmail.Create("support@example.com");
            var supportName = UserName.Create("Test");

            var supportDto = new GetSupportDto
            {
                UserId = supportId.Value,
                UserName = "Test",
                UserEmail = supportEmail.Value
            };

            var supportEntity = new Supports
            {
                UserId = supportId,
                UserName = supportName,
                UserEmail = supportEmail
            };

            var mockCursor = new Mock<IAsyncCursor<GetSupportDto>>();
            mockCursor.SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)
                .ReturnsAsync(false);
            mockCursor.SetupGet(c => c.Current).Returns(new List<GetSupportDto> { supportDto });

            _mongoCollectionMock.Setup(c => c.FindAsync(
                    It.IsAny<FilterDefinition<Supports>>(),
                    It.IsAny<FindOptions<Supports, GetSupportDto>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockCursor.Object);

            _mapperMock.Setup(m => m.Map<Supports>(supportDto)).Returns(supportEntity);

            var result = await _repository.GetSupportByIdAsync(supportId);

            Assert.NotNull(result);
            Assert.Equal(supportId.Value, result.UserId.Value);
            Assert.Equal("Test", result.UserName.Value);
            Assert.Equal("support@example.com", result.UserEmail.Value);
        }
    }
}
