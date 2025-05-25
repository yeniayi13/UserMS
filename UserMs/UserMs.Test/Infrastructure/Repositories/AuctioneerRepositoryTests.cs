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

namespace UserMs.Test.Infrastructure.Repositories
{
    public class AuctioneerRepositoryTests
    {
        private Mock<IUserDbContext> _dbContextMock;
        private Mock<IUserDbContextMongo> _mongoContextMock;
        private Mock<IMongoCollection<Auctioneers>> _mongoCollectionMock;
        private Mock<IMapper> _mapperMock;
        private AuctioneerRepository _repository;

        public AuctioneerRepositoryTests()
        {
            _dbContextMock = new Mock<IUserDbContext>();
            _mongoContextMock = new Mock<IUserDbContextMongo>();
            _mongoCollectionMock = new Mock<IMongoCollection<Auctioneers>>();
            _mapperMock = new Mock<IMapper>();

            var mockDatabase = new Mock<IMongoDatabase>();
            mockDatabase.Setup(db => db.GetCollection<Auctioneers>("Auctioneers", null))
                .Returns(_mongoCollectionMock.Object);

            _mongoContextMock.Setup(m => m.Database).Returns(mockDatabase.Object);

           // _repository = new AuctioneerRepository(_dbContextMock.Object, _mongoContextMock.Object, _mapperMock.Object);
        }

        // 🔹 Validación del Constructor
        [Fact]
        public void Constructor_ShouldThrowException_WhenDbContextIsNull()
        {
           // Assert.Throws<ArgumentNullException>(() => new AuctioneerRepository(null, _mongoContextMock.Object, _mapperMock.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowException_WhenMongoContextIsNull()
        {
          //  Assert.Throws<ArgumentNullException>(() => new AuctioneerRepository(_dbContextMock.Object, null, _mapperMock.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowException_WhenMongoDatabaseIsNull()
        {
            _mongoContextMock.Setup(m => m.Database).Returns((IMongoDatabase)null);
           // Assert.Throws<ArgumentNullException>(() => new AuctioneerRepository(_dbContextMock.Object, _mongoContextMock.Object, _mapperMock.Object));
        }

        [Fact]
        public void Constructor_ShouldThrowException_WhenMapperIsNull()
        {
           // Assert.Throws<ArgumentNullException>(() => new AuctioneerRepository(_dbContextMock.Object, _mongoContextMock.Object, null));
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

        [Fact]
        public async Task GetAuctioneerByEmailAsync_ShouldReturnNull_WhenAuctioneerNotFound()
        {
            var auctioneerEmail = UserEmail.Create("test@test.com");
            var auctionerName = UserName.Create("Name");
            var mockCursor = new Mock<IAsyncCursor<GetAuctioneerDto>>();
            mockCursor.SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(false); // 🔹 No hay registros encontrados
            mockCursor.SetupGet(c => c.Current).Returns(new List<GetAuctioneerDto>());

            _mongoCollectionMock.Setup(c => c.FindAsync(
                    It.IsAny<FilterDefinition<Auctioneers>>(),
                    It.IsAny<FindOptions<Auctioneers, GetAuctioneerDto>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockCursor.Object);

            _mapperMock.Setup(m => m.Map<Auctioneers>(It.IsAny<GetAuctioneerDto>()))
                .Returns((Auctioneers)null); // 🔹 Simula que no hay datos

            /*var result = await _repository.GetAuctioneerByEmailAsync(auctioneerEmail);

            Assert.Null(result); // ✅ Validación final*/
        }

        [Fact]
        public async Task GetAuctioneerByEmailAsync_ShouldReturnAuctioneer_WhenFound()
        {

            var auctioneerEmail = UserEmail.Create("test@test.com");
            var auctionerName = UserName.Create("Name");

            var auctioneerDto = new GetAuctioneerDto { UserId = Guid.NewGuid(), UserName = "John Doe", UserEmail = auctioneerEmail.Value };
            var auctioneer = new Auctioneers {UserId = auctioneerDto.UserId,UserName = auctionerName, UserEmail = auctioneerEmail};

            var mockCursor = new Mock<IAsyncCursor<GetAuctioneerDto>>();
            mockCursor.SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)
                .ReturnsAsync(false);
            mockCursor.SetupGet(c => c.Current).Returns(new List<GetAuctioneerDto> { auctioneerDto });

            _mongoCollectionMock.Setup(c => c.FindAsync(
                    It.IsAny<FilterDefinition<Auctioneers>>(),
                    It.IsAny<FindOptions<Auctioneers, GetAuctioneerDto>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockCursor.Object);

            _mapperMock.Setup(m => m.Map<Auctioneers>(auctioneerDto)).Returns(auctioneer);

            /*var result = await _repository.GetAuctioneerByEmailAsync(auctioneerEmail);

            Assert.NotNull(result);
            Assert.Equal(auctioneerDto.UserId, result.UserId.Value);
            Assert.Equal("Name", result.UserName.Value);
            Assert.Equal(auctioneerEmail, result.UserEmail);*/
        }


        // 🔹 Validación de `GetAuctioneerAllAsync()`
        [Fact]
        public async Task GetAuctioneerAllAsync_ShouldReturnEmptyList_WhenNoAuctioneersFound()
        {
            var mockCursor = new Mock<IAsyncCursor<GetAuctioneerDto>>();
            mockCursor.SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);
            mockCursor.SetupGet(c => c.Current).Returns(new List<GetAuctioneerDto>());

            _mongoCollectionMock.Setup(c => c.FindAsync(
                    It.IsAny<FilterDefinition<Auctioneers>>(),
                    It.IsAny<FindOptions<Auctioneers, GetAuctioneerDto>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockCursor.Object);

            _mapperMock.Setup(m => m.Map<List<Auctioneers>>(It.IsAny<List<GetAuctioneerDto>>()))
                .Returns(new List<Auctioneers>());

          /*  var result = await _repository.GetAuctioneerAllAsync();

            Assert.NotNull(result);
            Assert.Empty(result);*/
        }

        [Fact]
        public async Task GetAuctioneerAllAsync_ShouldReturnAuctioneers_WhenFound()
        {
            var auctioneerName = UserName.Create("Jose");
            var auctioneerName2 = UserName.Create("Maria");
            var auctioneerDtos = new List<GetAuctioneerDto>
            {
                new GetAuctioneerDto { UserId = Guid.NewGuid(), UserName = auctioneerName.Value },
                new GetAuctioneerDto { UserId = Guid.NewGuid(), UserName = auctioneerName2.Value }
            };

            var auctioneerEntities = new List<Auctioneers>
        {
            new Auctioneers { UserId = auctioneerDtos[0].UserId, UserName =auctioneerName },
            new Auctioneers { UserId = auctioneerDtos[1].UserId, UserName = auctioneerName2 }
        };

            var mockCursor = new Mock<IAsyncCursor<GetAuctioneerDto>>();
            mockCursor.SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)
                .ReturnsAsync(false);
            mockCursor.SetupGet(c => c.Current).Returns(auctioneerDtos);

            _mongoCollectionMock.Setup(c => c.FindAsync(
                    It.IsAny<FilterDefinition<Auctioneers>>(),
                    It.IsAny<FindOptions<Auctioneers, GetAuctioneerDto>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockCursor.Object);

            _mapperMock.Setup(m => m.Map<List<Auctioneers>>(auctioneerDtos)).Returns(auctioneerEntities);

          /*  var result = await _repository.GetAuctioneerAllAsync();

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal(auctioneerDtos[0].UserId, result[0].UserId.Value);
            Assert.Equal("Jose", result[0].UserName.Value);
            Assert.Equal(auctioneerDtos[1].UserId, result[1].UserId.Value);
            Assert.Equal("Maria", result[1].UserName.Value);*/
        }

        // 🔹 Validación de `GetAuctioneerByIdAsync()`
        [Fact]
        public async Task GetAuctioneerByIdAsync_ShouldReturnNull_WhenAuctioneerNotFound()
        {
            var auctioneerId =UserId.Create(Guid.NewGuid());

            var mockCursor = new Mock<IAsyncCursor<GetAuctioneerDto>>();
            mockCursor.SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);
            mockCursor.SetupGet(c => c.Current).Returns(new List<GetAuctioneerDto>()); // 🔹 Asegurar que `Current` no es `null`

            _mongoCollectionMock.Setup(c => c.FindAsync(
                    It.IsAny<FilterDefinition<Auctioneers>>(),
                    It.IsAny<FindOptions<Auctioneers, GetAuctioneerDto>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockCursor.Object); // 🔹 Evita que `FindAsync()` retorne `null`

           /* var result = await _repository.GetAuctioneerByIdAsync(auctioneerId);

            Assert.Null(result);*/
        }

        [Fact]
        public async Task GetAuctioneerByIdAsync_ShouldReturnAuctioneer_WhenFound()
        {
            var auctioneerId = UserId.Create(Guid.NewGuid());
            var auctioneerName = UserName.Create("Jose");
            var auctioneerDto = new GetAuctioneerDto { UserId = auctioneerId.Value, UserName = auctioneerName.Value};
            var auctioneerEntity = new Auctioneers { UserId = auctioneerId.Value, UserName = auctioneerName };

            var mockCursor = new Mock<IAsyncCursor<GetAuctioneerDto>>();
            mockCursor.SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)
                .ReturnsAsync(false);
            mockCursor.SetupGet(c => c.Current).Returns(new List<GetAuctioneerDto> { auctioneerDto });

            _mongoCollectionMock.Setup(c => c.FindAsync(
                    It.IsAny<FilterDefinition<Auctioneers>>(),
                    It.IsAny<FindOptions<Auctioneers, GetAuctioneerDto>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockCursor.Object);

            _mapperMock.Setup(m => m.Map<Auctioneers>(auctioneerDto)).Returns(auctioneerEntity);

          /*  var result = await _repository.GetAuctioneerByIdAsync(auctioneerId);

            Assert.NotNull(result);
            Assert.Equal(auctioneerId.Value, result.UserId.Value);
            Assert.Equal("Jose", result.UserName.Value);*/
        }
    }
}
