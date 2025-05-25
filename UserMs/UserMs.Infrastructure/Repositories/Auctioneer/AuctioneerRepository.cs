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
using UserMs.Domain.Entities;
using UserMs.Domain.Entities.Auctioneer;
using UserMs.Domain.Entities.Auctioneer.ValueObjects;
using UserMs.Domain.Entities.Bidder;
using UserMs.Domain.Entities.UserEntity;

namespace UserMs.Infrastructure.Repositories.Auctioneer
{
    public class AuctioneerRepository : IAuctioneerRepository
    {
        private readonly IUserDbContext _dbContext;
      
        private readonly IMapper _mapper;

        public AuctioneerRepository(IUserDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));

        }


        public async Task AddAsync(Auctioneers auctioneer)
        {
            await _dbContext.Auctioneers.AddAsync(auctioneer);
            await _dbContext.SaveEfContextChanges("");
        }

        public async Task<Auctioneers?> UpdateAsync(UserId auctioneerId, Auctioneers auctioneer)
        {
            _dbContext.Auctioneers.Update(auctioneer);
            await _dbContext.SaveEfContextChanges("");
            return auctioneer;
        }

        public async Task<Auctioneers?> DeleteAsync(UserId auctioneerId)
        {
            var existingAuctioneers = await _dbContext.Auctioneers.FindAsync(auctioneerId);
            await _dbContext.SaveEfContextChanges("");
            return existingAuctioneers;
        }
    }
}
