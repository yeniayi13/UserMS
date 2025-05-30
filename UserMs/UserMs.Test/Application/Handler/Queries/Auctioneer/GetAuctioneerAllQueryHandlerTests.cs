using AutoMapper;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Application.Handlers.Auctioneer.Queries;
using UserMs.Application.Queries.Auctioneer;
using UserMs.Commoon.Dtos.Users.Response.Auctioneer;
using UserMs.Core.Repositories.Auctioneer;
using UserMs.Domain.Entities.Auctioneer;
using UserMs.Domain.Entities.IUser.ValueObjects;
using Xunit;

namespace UserMs.Test.Application.Handler.Queries.Auctioneer
{
    public class GetAuctioneerAllQueryHandlerTests
    {
        private readonly Mock<IAuctioneerRepositoryMongo> _auctioneerRepositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly GetAuctioneerAllQueryHandler _handler;

        public GetAuctioneerAllQueryHandlerTests()
        {
            _auctioneerRepositoryMock = new Mock<IAuctioneerRepositoryMongo>();
            _mapperMock = new Mock<IMapper>();

            _handler = new GetAuctioneerAllQueryHandler(
                _auctioneerRepositoryMock.Object,
                _mapperMock.Object
            );
        }

        /// <summary>
        /// Verifica que cuando hay subastadores, se devuelvan correctamente los datos mapeados.
        /// </summary>
        [Fact]
        public async Task Handle_ShouldReturnMappedAuctioneers_WhenAuctioneersExist()
        {
            // Arrange
            var name1 = UserName.Create("Subastador 1");
            var name2 = UserName.Create("Subastador 2");
            var auctioneers = new List<Auctioneers>
        {
            new Auctioneers { UserId = Guid.NewGuid(), UserName = name1 },
            new Auctioneers { UserId = Guid.NewGuid(), UserName = name2 }
        };
            var expectedDtos = new List<GetAuctioneerDto>
        {
            new GetAuctioneerDto { UserId = auctioneers[0].UserId, UserName = auctioneers[0].UserName.Value },
            new GetAuctioneerDto { UserId = auctioneers[1].UserId, UserName = auctioneers[1].UserName.Value }
        };

            _auctioneerRepositoryMock
                .Setup(repo => repo.GetAuctioneerAllAsync())
                .ReturnsAsync(auctioneers);

            _mapperMock
                .Setup(mapper => mapper.Map<List<GetAuctioneerDto>>(auctioneers))
                .Returns(expectedDtos);

            // Act
            var result = await _handler.Handle(new GetAuctioneerAllQuery(), CancellationToken.None);

            // Assert
            Assert.Equal(expectedDtos.Count, result.Count);
            Assert.Equal(expectedDtos[0].UserId, result[0].UserId);
            Assert.Equal(expectedDtos[1].UserName, result[1].UserName);
        }

        /// <summary>
        /// Verifica que cuando no hay subastadores, se retorne una lista vacía.
        /// </summary>
        [Fact]
        public async Task Handle_ShouldReturnEmptyList_WhenNoAuctioneersFound()
        {
            // Arrange
            _auctioneerRepositoryMock
                .Setup(repo => repo.GetAuctioneerAllAsync())
                .ReturnsAsync(new List<Auctioneers>());

            // Act
            var result = await _handler.Handle(new GetAuctioneerAllQuery(), CancellationToken.None);

            // Assert
            Assert.Empty(result);
        }

        /// <summary>
        /// Verifica que cuando ocurre una excepción en el repositorio, se propaga correctamente.
        /// </summary>
        [Fact]
        public async Task Handle_ShouldReturnEmptyList_WhenRepositoryFails()
        {
            // Arrange
            _auctioneerRepositoryMock
                .Setup(repo => repo.GetAuctioneerAllAsync())
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _handler.Handle(new GetAuctioneerAllQuery(), CancellationToken.None);

            // Assert
            Assert.Empty(result);
        }
    }

}
