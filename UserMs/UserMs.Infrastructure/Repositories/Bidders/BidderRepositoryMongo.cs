using AutoMapper;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Commoon.Dtos.Users.Response.Bidder;
using UserMs.Core.Database;
using UserMs.Core.Repositories.Biddere;
using UserMs.Core.Repositories.Bidders;
using UserMs.Domain.Entities;

namespace UserMs.Infrastructure.Repositories.Bidders
{
    public class BidderRepositoryMongo : IBidderRepositoryMongo
    {
       
        private readonly IMongoCollection<Domain.Entities.Bidder.Bidders> _collection;
        private readonly IMapper _mapper;

        public BidderRepositoryMongo( IUserDbContextMongo context, IMapper mapper)
        {
           
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));

            if (context?.Database == null)
            {
                throw new ArgumentNullException(nameof(context.Database));
            }

            _collection = context.Database.GetCollection<Domain.Entities.Bidder.Bidders>("Bidders");
        }

        public async Task<List<Domain.Entities.Bidder.Bidders>> GetBidderAllAsync()
        {
            var projection = Builders<Domain.Entities.Bidder.Bidders>.Projection.Exclude("_id").Exclude("IsDeleted")
                .Exclude("CreatedAt").Exclude("CreatedBy").Exclude("UserPassword").Exclude("BidderDelete");

            var bidderDto = await _collection.Find(Builders<Domain.Entities.Bidder.Bidders>.Filter.Empty)
                .Project<GetBidderDto>(projection)
                .ToListAsync()
                .ConfigureAwait(false);

            return bidderDto == null || bidderDto.Count == 0 ? new List<Domain.Entities.Bidder.Bidders>() : _mapper.Map<List<Domain.Entities.Bidder.Bidders>>(bidderDto);
        }

        public async Task<Domain.Entities.Bidder.Bidders?> GetBidderByIdAsync(UserId bidderId)
        {
            var filter = Builders<Domain.Entities.Bidder.Bidders>.Filter.Eq("UserId", bidderId.Value);
            var projection = Builders<Domain.Entities.Bidder.Bidders>.Projection.Exclude("_id").Exclude("IsDeleted")
                .Exclude("CreatedAt").Exclude("CreatedBy").Exclude("UserPassword").Exclude("BidderDelete");

            var bidderDto = await _collection.Find(filter).Project<GetBidderDto>(projection).FirstOrDefaultAsync().ConfigureAwait(false);
            return _mapper.Map<Domain.Entities.Bidder.Bidders>(bidderDto);
        }

        public async Task<Domain.Entities.Bidder.Bidders?> GetBidderByEmailAsync(UserEmail bidderUserId)
        {
            var filter = Builders<Domain.Entities.Bidder.Bidders>.Filter.Eq("UserEmail", bidderUserId.Value);
            var projection = Builders<Domain.Entities.Bidder.Bidders>.Projection.Exclude("_id").Exclude("IsDeleted")
                .Exclude("CreatedAt").Exclude("CreatedBy").Exclude("UserPassword").Exclude("BidderDelete");

            var bidderDto = await _collection.Find(filter).Project<GetBidderDto>(projection).FirstOrDefaultAsync().ConfigureAwait(false);
            return _mapper.Map<Domain.Entities.Bidder.Bidders>(bidderDto);
        }

       
    }
}
