using AutoMapper;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserMs.Commoon.Dtos.Users.Response.Support;
using UserMs.Core.Database;
using UserMs.Core.Repositories.SupportsRepo;
using UserMs.Domain.Entities;
using UserMs.Domain.Entities.Auctioneer.ValueObjects;
using UserMs.Domain.Entities.Support;
using UserMs.Domain.Entities.Support.ValueObjects;

namespace UserMs.Infrastructure.Repositories.Support
{
    public class SupportRepository : ISupportRepository
    {
        private readonly IUserDbContext _dbContext;
     
        private readonly IMapper _mapper;

        public SupportRepository(IUserDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));

        }

      
        public async Task AddAsync(Domain.Entities.Support.Supports support)
        {
            await _dbContext.Supports.AddAsync(support);
            await _dbContext.SaveEfContextChanges("");
        }

        public async Task<Domain.Entities.Support.Supports?> UpdateAsync(UserId supportId, Domain.Entities.Support.Supports support)
        {
            _dbContext.Supports.Update(support);
            await _dbContext.SaveEfContextChanges("");
            return support;
        }

        public async Task<Domain.Entities.Support.Supports?> DeleteAsync(UserId supportId)
        {
            var existingSupports = await _dbContext.Supports.FindAsync(supportId);
            await _dbContext.SaveEfContextChanges("");
            return existingSupports;
        }
    }
}
