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
using UserMs.Domain.Entities;
using UserMs.Domain.Entities.Auctioneer;
using UserMs.Domain.Entities.Auctioneer.ValueObjects;
using UserMs.Domain.Entities.Bidder;
using UserMs.Domain.Entities.Bidder.ValueObjects;

namespace UserMs.Infrastructure.Repositories.Bidder
{
    public class BidderRepository : IBidderRepository
    {
        private readonly IUserDbContext _dbContext;
     
        private readonly IMapper _mapper;

        public BidderRepository(IUserDbContext dbContext,  IMapper mapper)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));


        }


        public async Task AddAsync(Domain.Entities.Bidder.Bidders bidder)
        {
            await _dbContext.Bidders.AddAsync(bidder);
            await _dbContext.SaveEfContextChanges("");
        }

        public async Task<Domain.Entities.Bidder.Bidders> UpdateAsync(UserId bidderId, Domain.Entities.Bidder.Bidders bidder)
        {
            _dbContext.Bidders.Update(bidder);
            await _dbContext.SaveEfContextChanges("");
            return bidder;
        }

        public async Task<Domain.Entities.Bidder.Bidders> DeleteAsync(UserId bidderId)
        {

            var existingBidders = await _dbContext.Bidders.FindAsync(bidderId);
            await _dbContext.SaveEfContextChanges("");
            return existingBidders;
        }
    }
}
