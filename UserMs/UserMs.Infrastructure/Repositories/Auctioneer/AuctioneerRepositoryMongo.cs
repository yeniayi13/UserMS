using AutoMapper;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Commoon.Dtos.Users.Response.Auctioneer;
using UserMs.Core.Database;
using UserMs.Core.Repositories.Auctioneer;
using UserMs.Domain.Entities.Auctioneer;
using UserMs.Domain.Entities;

namespace UserMs.Infrastructure.Repositories.Auctioneer
{
    public class AuctioneerRepositoryMongo  : IAuctioneerRepositoryMongo
    {
       
        private readonly IMongoCollection<Auctioneers> _collection;
        private readonly IMapper _mapper;

        public AuctioneerRepositoryMongo( IUserDbContextMongo context, IMapper mapper)
        {
            
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));

            if (context?.Database == null)
            {
                throw new ArgumentNullException(nameof(context.Database));
            }

            _collection = context.Database.GetCollection<Auctioneers>("Auctioneers");
        }

        public async Task<List<Auctioneers>> GetAuctioneerAllAsync()
        {
            var projection = Builders<Auctioneers>.Projection.Exclude("_id").Exclude("IsDeleted").Exclude("CreatedAt").Exclude("CreatedBy");

            var auctioneerDto = await _collection.Find(Builders<Auctioneers>.Filter.Empty)
                .Project<GetAuctioneerDto>(projection)
                .ToListAsync()
                .ConfigureAwait(false);

            return auctioneerDto == null || auctioneerDto.Count == 0 ? new List<Auctioneers>() : _mapper.Map<List<Auctioneers>>(auctioneerDto);
        }

        public async Task<Auctioneers?> GetAuctioneerByIdAsync(UserId auctioneerId)
        {
            var filter = Builders<Auctioneers>.Filter.Eq("UserId", auctioneerId.Value);
            var projection = Builders<Auctioneers>.Projection.Exclude("_id").Exclude("IsDeleted").Exclude("CreatedAt").Exclude("CreatedBy");

            var auctioneerDto = await _collection.Find(filter).Project<GetAuctioneerDto>(projection).FirstOrDefaultAsync().ConfigureAwait(false);
            return _mapper.Map<Auctioneers>(auctioneerDto);
        }

        public async Task<Auctioneers?> GetAuctioneerByEmailAsync(UserEmail auctioneerUserId)
        {
            var filter = Builders<Auctioneers>.Filter.Eq("UserEmail", auctioneerUserId.Value);
            var projection = Builders<Auctioneers>.Projection.Exclude("_id").Exclude("IsDeleted").Exclude("CreatedAt").Exclude("CreatedBy");

            var auctioneerDto = await _collection.Find(filter).Project<GetAuctioneerDto>(projection).FirstOrDefaultAsync().ConfigureAwait(false);
            return _mapper.Map<Auctioneers>(auctioneerDto);
        }

    
    }
}
